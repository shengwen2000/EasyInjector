using EasyApiProxys.Options;
using System.Text.Json;
using System.Text;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Globalization;

namespace EasyApiProxys
{
    /// <summary>
    /// Kmuhome API 協定
    /// </summary>
    public static class KmuhomeApiExtension
    {
        /// <summary>
        /// Header Name 回應代號 X_Api_Result
        /// </summary>
        public const string HeaderName_Result = KmuhomeApiConstants.Header_Result;

        /// <summary>
        /// Header Name 資料型別 X_Api_DataType
        /// </summary>
        public const string HeaderName_DataType = KmuhomeApiConstants.Header_DataType;

        /// <summary>
        /// 舊版 Header Name 回應代號
        /// </summary>
        public const string HeaderName_Result_Legacy = KmuhomeApiConstants.Header_Result_Legacy;

        /// <summary>
        /// 舊版 Header Name 資料型別
        /// </summary>
        public const string HeaderName_DataType_Legacy = KmuhomeApiConstants.Header_DataType_Legacy;

        /// <summary>
        /// Kmuhome Api 預設 JsonSerializer Options
        /// 驼峰命名 日期(無時區與毫秒) 2024-08-06T15:18:41 UnsafeRelaxedJsonEscaping Enum 為小寫字串
        /// </summary>
        public static JsonSerializerOptions DefaultJsonOptions { get; private set; }

        static KmuhomeApiExtension()
        {
            DefaultJsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            DefaultJsonOptions.Converters.Add(new DateTimeConverter());
            DefaultJsonOptions.Converters.Add(new JsonEnumFactory());
        }

        /// <summary>
        /// 使用KmuhomeApi通訊方法
        /// </summary>
        /// <param name="baseUrl">共用的HttpClient KeyName</param>
        /// <param name="builder">builder</param>
        /// <param name="defaltTimeoutSeconds">預設逾時秒數</param>
        static public ApiProxyBuilder UseKmuhomeApiProtocol(this ApiProxyBuilder builder, string baseUrl, int defaltTimeoutSeconds = 15)
        {
            var handler = new KmuhomeApiHandler(builder.Options.Step2, builder.Options.Step3);
            builder.Options.Handlers.Add(handler);
            builder.Options.JsonOptions = DefaultJsonOptions;
            builder.Options.Step2 = handler.Step2;
            builder.Options.Step3 = handler.Step3;
            builder.Options.BaseUrl = baseUrl;
            builder.Options.DefaultTimeout = TimeSpan.FromSeconds(defaltTimeoutSeconds);
            return builder;
        }

        internal class KmuhomeApiHandler(
            Func<StepContext, Task>? step2,
            Func<StepContext, Task>? step3)
        {
            private const string RESULT_OK = KmuhomeApiConstants.Code_OK;
            private const string RESULT_IM = KmuhomeApiConstants.Code_IM;
            private const string RESULT_NON_DEFAULT_API_RESULT = KmuhomeApiConstants.Code_NonDefault;

            public async Task Step2(StepContext step)
            {
                if (step2 != null)
                    await step2(step).ConfigureAwait(false);

                var req = step.Request!;
                var _options = step.BuilderOptions;

                req.Method = HttpMethod.Post;

                // 方法的第一個參數 會當成Json內容進行傳送
                if (step.Invocation.Arguments.Length != 0)
                {
                    var jsontext = JsonSerializer.Serialize(step.Invocation.Arguments.ElementAt(0), _options.JsonOptions);
                    req.Content = new StringContent(jsontext, Encoding.UTF8, "application/json");
                }
            }

            public async Task Step3(StepContext step)
            {
                if (step3 != null)
                    await step3(step).ConfigureAwait(false);

                var resp = step.Response!;
                var invocation = step.Invocation;
                var _options = step.BuilderOptions;

                // 如果沒有回應標頭 那應該是沒有授權或是404之類的錯誤
                if (!resp.Headers.Contains(HeaderName_Result) && !resp.Headers.Contains(HeaderName_Result_Legacy))
                {
                    // 有http錯誤碼 拋出 HttpRequestException
                    resp.EnsureSuccessStatusCode();
                    // 沒有http錯誤碼 但沒有標頭 拋出無法解析錯誤
                    throw new ApiCodeException(RESULT_NON_DEFAULT_API_RESULT, "回應內容非協議格式無法解析");
                }

                // 標題含有Result資訊 預先載入 (優先讀取標準格式，若無則讀取舊版)
                string resultCode = RESULT_OK;
                if (resp.Headers.TryGetValues(HeaderName_Result, out var values))
                    resultCode = values.First();
                else if (resp.Headers.TryGetValues(HeaderName_Result_Legacy, out values))
                    resultCode = values.First();

                // 取得回應內容
                using var s1 = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);

                // 方法已經確定回傳值(void | task)
                if (invocation.Method.ReturnType == typeof(void) || invocation.Method.ReturnType == typeof(Task))
                {
                    // ok
                    if (resultCode.Equals(RESULT_OK, StringComparison.OrdinalIgnoreCase))
                        return;

                    // not ok
                    await HandleException(resp, _options, resultCode, s1).ConfigureAwait(false);
                }
                // 方法有回傳值Task<T>
                else if (invocation.Method.ReturnType.IsGenericType && invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    // ok
                    if (resultCode.Equals(RESULT_OK, StringComparison.OrdinalIgnoreCase))
                    {
                        // no content
                        if (resp.StatusCode == HttpStatusCode.NoContent)
                            return;

                        // Task<t1>
                        var dataType = invocation.Method.ReturnType.GetGenericArguments()[0];
                        var ret = JsonSerializer.Deserialize(s1, dataType, _options.JsonOptions);
                        step.Result = ret;
                        return;
                    }

                    // not ok
                    await HandleException(resp, _options, resultCode, s1).ConfigureAwait(false);
                }
                // 方法有回傳值(string Account ...)
                else
                {
                    // ok
                    if (resultCode.Equals(RESULT_OK, StringComparison.OrdinalIgnoreCase))
                    {
                        // no content
                        if (resp.StatusCode == HttpStatusCode.NoContent)
                            return;

                        var dataType = invocation.Method.ReturnType;
                        var ret = JsonSerializer.Deserialize(s1, dataType, _options.JsonOptions);
                        step.Result = ret;
                        return;
                    }
                    // not ok
                    await HandleException(resp, _options, resultCode, s1).ConfigureAwait(false);
                }
            }

            /// <summary>
            /// 處理錯誤回應
            /// - 明確收到非OK 回應 將以 ApiCodeException 拋出
            /// </summary>
            private static async Task HandleException(HttpResponseMessage resp, ApiProxyBuilderOptions options, string resultCode, Stream s1)
            {
                // 將s1 讀取為字串
                var respText = await new StreamReader(s1).ReadToEndAsync().ConfigureAwait(false);
                var ret = JsonSerializer.Deserialize<DefaultApiResult<JsonNode>>(respText, options.JsonOptions);
                ApiCodeException? retEx;

                if (ret == null)
                    retEx = new ApiCodeException(RESULT_NON_DEFAULT_API_RESULT, $"回應內容預期為JSON回應無法解析 {respText}");
                else
                    retEx = new ApiCodeException(ret.Result, ret.Message, ret.Data);

                SetDebugInfo(retEx, resp);

                throw retEx;
            }

            /// <summary>
            /// 設定偵錯資訊
            /// </summary>
            private static void SetDebugInfo(ApiCodeException retEx, HttpResponseMessage resp)
            {
                string? traceId = null;
                // 嘗試取得常用的 Trace ID Header
                if (resp.Headers.TryGetValues("X-Trace-Id", out IEnumerable<string>? values) ||
                    resp.Headers.TryGetValues("X-Request-Id", out values) ||
                    resp.Headers.TryGetValues("X-Correlation-Id", out values))
                {
                    traceId = values.FirstOrDefault();
                }

                retEx.StatusCode = (int)resp.StatusCode;
                retEx.HttpMethod = resp.RequestMessage?.Method.Method;
                retEx.TargetUrl = resp.RequestMessage?.RequestUri?.ToString();
                retEx.TraceId = traceId;
            }
        }

        /// <summary>
        /// JSON日期格式
        /// </summary>
        public class DateTimeConverter : JsonConverter<DateTime>
        {
            /// <summary>
            /// 讀取
            /// </summary>
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return DateTime.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// 寫入
            /// </summary>
            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss", CultureInfo.InvariantCulture));
            }
        }

        public class JsonEnumFactory : JsonConverterFactory
        {
            /// <summary>
            /// 確定此轉換器是否可以轉換指定類型的物件。
            /// </summary>
            /// <param name="typeToConvert">要檢查轉換能力的類型。</param>
            /// <returns>
            /// 如果轉換器可以轉換指定的類型，則為 <c>true</c>；否則為 <c>false</c>。
            /// 在此實現中，如果類型是列舉，則返回 <c>true</c>。
            /// </returns>
            public override bool CanConvert(Type typeToConvert)
            {
                return typeToConvert.IsEnum;
            }

            public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            {
                var converterType = typeof(JsonEnumConverter<>).MakeGenericType(typeToConvert);
                return Activator.CreateInstance(converterType) as JsonConverter ?? throw new NotSupportedException(typeToConvert.FullName);
            }
        }

        public class JsonEnumConverter<T> : JsonConverter<T> where T : struct, Enum
        {
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var enumString = reader.GetString();
                if (Enum.TryParse(enumString, true, out T value))
                    return value;
                throw new JsonException($"Unable to convert \"{enumString}\" to Enum \"{typeof(T)}\".");
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString().ToLower());
            }
        }
    }
}
