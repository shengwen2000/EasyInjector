using EasyApiProxys.Options;
using System.Text.Json;
using System.Text;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using static EasyApiProxys.DefaultApiExtension;

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
        public const string HeaderName_Result = "X_Api_Result";

        /// <summary>
        /// Header Name 資料型別 X_Api_DataType
        /// </summary>
        public const string HeaderName_DataType = "X_Api_DataType";

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
            private const string RESULT_OK = "ok";
            private const string RESULT_IM = "im";
            private const string RESULT_NON_DEFAULT_API_RESULT = "non_default_api_result";

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

                // NOT OK 拋送 HttpRequestException
                resp.EnsureSuccessStatusCode();

                // 標題含有Result資訊 預先載入
                var resultCode = (resp.Headers.GetValues(HeaderName_Result).SingleOrDefault() ?? RESULT_OK).ToLower();

                // 取得回應內容
                var s1 = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);

                // 方法沒有回傳值(void | task)
                if (invocation.Method.ReturnType == typeof(void) || invocation.Method.ReturnType == typeof(Task))
                {
                    // ok
                    if (resultCode.Equals(RESULT_OK, StringComparison.OrdinalIgnoreCase))
                        return;
                    // im
                    if (resultCode.Equals(RESULT_IM, StringComparison.OrdinalIgnoreCase))
                    {
                        var ret = JsonSerializer.Deserialize<DefaultApiResult<JsonNode>>(s1, _options.JsonOptions)
                            ?? throw new ApiCodeException(RESULT_NON_DEFAULT_API_RESULT, "回應內容非 DefaultApiResult 格式無法解析");
                        throw new ApiCodeException(ret.Result, ret.Message, ret.Data);
                    }
                    // not ok
                    {
                        var ret = JsonSerializer.Deserialize<DefaultApiResult<object>>(s1, _options.JsonOptions)
                            ?? throw new ApiCodeException(RESULT_NON_DEFAULT_API_RESULT, "回應內容非 DefaultApiResult 格式無法解析");
                        throw new ApiCodeException(ret.Result, ret.Message, ret.Data);
                    }
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

                    // im
                    if (resultCode.Equals(RESULT_IM, StringComparison.OrdinalIgnoreCase))
                    {
                        var ret = JsonSerializer.Deserialize<DefaultApiResult<JsonNode>>(s1, _options.JsonOptions)
                            ?? throw new ApiCodeException(RESULT_NON_DEFAULT_API_RESULT, "回應內容非 DefaultApiResult 格式無法解析");
                        throw new ApiCodeException(ret.Result, ret.Message, ret.Data);
                    }
                    // not ok
                    else
                    {
                        var ret = JsonSerializer.Deserialize<DefaultApiResult<object>>(s1, _options.JsonOptions)
                            ?? throw new ApiCodeException(RESULT_NON_DEFAULT_API_RESULT, "回應內容非 DefaultApiResult 格式無法解析");
                        throw new ApiCodeException(ret.Result, ret.Message, ret.Data);
                    }
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
                    // im
                    if (resultCode.Equals(RESULT_IM, StringComparison.OrdinalIgnoreCase))
                    {
                        var ret = JsonSerializer.Deserialize<DefaultApiResult<JsonNode>>(s1, _options.JsonOptions)
                            ?? throw new ApiCodeException(RESULT_NON_DEFAULT_API_RESULT, "回應內容非 DefaultApiResult 格式無法解析");
                        throw new ApiCodeException(ret.Result, ret.Message, ret.Data);
                    }
                    // not ok
                    {
                        var ret = JsonSerializer.Deserialize<DefaultApiResult<object>>(s1, _options.JsonOptions)
                            ?? throw new ApiCodeException(RESULT_NON_DEFAULT_API_RESULT, "回應內容非 DefaultApiResult 格式無法解析");
                        throw new ApiCodeException(ret.Result, ret.Message, ret.Data);
                    }
                }
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
