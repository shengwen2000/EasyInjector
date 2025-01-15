using EasyApiProxys.Options;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Globalization;
using System.Net;
using System.Text.Json.Nodes;

namespace EasyApiProxys
{
    /// <summary>
    /// 預設的API 協定
    /// </summary>
    public static class DefaultApiExtension
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
        /// Default Api 預設 JsonSerializer Options
        /// 驼峰命名 日期(無時區與毫秒) 2024-08-06T15:18:41 UnsafeRelaxedJsonEscaping Enum 為小寫
        /// </summary>
        public static JsonSerializerOptions DefaultJsonOptions { get; private set; }

        static DefaultApiExtension()
        {
            DefaultJsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            DefaultJsonOptions.Converters.Add(new DateTimeConverter());
            DefaultJsonOptions.Converters.Add(new JsonLowerCaseEnumFactory());
        }

        /// <summary>
        /// 使用標準Api通訊方法
        /// </summary>
        /// <param name="baseUrl">共用的HttpClient KeyName</param>
        /// <param name="builder">builder</param>
        /// <param name="defaltTimeoutSeconds">預設逾時秒數</param>
        static public ApiProxyBuilder UseDefaultApiProtocol(this ApiProxyBuilder builder, string baseUrl, int defaltTimeoutSeconds = 15)
        {
            var hander = new DefaultApiHandler(builder.Options.Step2, builder.Options.Step3);
            builder.Options.JsonOptions = DefaultJsonOptions;
            builder.Options.Step2 = hander.Step2;
            builder.Options.Step3 = hander.Step3;
            builder.Options.BaseUrl = baseUrl;
            builder.Options.DefaultTimeout = TimeSpan.FromSeconds(defaltTimeoutSeconds);
            return builder;
        }

        internal class DefaultApiHandler(
            Func<StepContext, Task>? step2,
            Func<StepContext, Task>? step3)
        {
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
                if (resp.StatusCode != HttpStatusCode.OK)
                {
                    var msg = await resp.Content.ReadAsStringAsync();
                    throw new HttpRequestException(msg, null, resp.StatusCode);
                }

                // 標題含有Result資訊 預先載入 如果沒有那應該是舊的Api可用 OK 兼容之
                var resultCode = (resp.Headers.GetValues(HeaderName_Result).SingleOrDefault() ?? "ok").ToLower();

                // 取得回應內容
                var s1 = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);

                // 方法沒有回傳值(void | task)
                if (invocation.Method.ReturnType == typeof(void) || invocation.Method.ReturnType == typeof(Task))
                {
                    // 確定非OK
                    if (resultCode != "ok")
                    {
                        var ret = JsonSerializer.Deserialize<DefaultApiResult<JsonNode>>(s1, _options.JsonOptions)
                            ?? throw new ApiCodeException("non_default_api_result", "回應內容非 DefaultApiResult 格式無法解析");
                        ToLowerResult(ret);
                        throw new ApiCodeException(ret.Result, ret.Message, ret.Data);
                    }
                    // OK
                    else
                    {
                        var ret = JsonSerializer.Deserialize<DefaultApiResult>(s1, _options.JsonOptions)
                            ?? throw new ApiCodeException("non_default_api_result", "回應內容非 DefaultApiResult 格式無法解析");
                        ToLowerResult(ret);
                        // 兼容舊的沒有 X_Api_Result Header 新的此值必定為OK
                        if (ret.Result != "ok")
                            throw new ApiCodeException(ret.Result, ret.Message, ret.Data as JsonNode);
                    }
                }
                // 方法有回傳值Task<T>
                else if (invocation.Method.ReturnType.IsGenericType && invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    // 確定非OK
                    if (resultCode != "ok")
                    {
                        var ret = JsonSerializer.Deserialize<DefaultApiResult<JsonNode>>(s1, _options.JsonOptions)
                            ?? throw new ApiCodeException("non_default_api_result", "回應內容非 DefaultApiResult 格式無法解析");
                        ToLowerResult(ret);
                        throw new ApiCodeException(ret.Result, ret.Message, ret.Data);
                    }
                    // OK
                    else
                    {
                        // Task<t1>
                        var t1 = invocation.Method.ReturnType.GetGenericArguments()[0];
                        var resultT1 = typeof(DefaultApiResult<>).MakeGenericType([t1]);

                        var ret = JsonSerializer.Deserialize(s1, resultT1, _options.JsonOptions) as DefaultApiResult
                            ?? throw new ApiCodeException("non_default_api_result", "回應內容非 DefaultApiResult 格式無法解析");
                        ToLowerResult(ret);

                        // 兼容舊的沒有 X_Api_Result Header 新的此值必定為OK
                        if (ret.Result != "ok")
                            throw new ApiCodeException(ret.Result, ret.Message, ret.Data as JsonNode);

                        var data = ret.Data;
                        step.Result = data;
                    }
                }
                // 方法有回傳值(string Account ...)
                else
                {
                    // 確定非OK
                    if (resultCode != "ok")
                    {
                        var ret = JsonSerializer.Deserialize<DefaultApiResult<JsonNode>>(s1, _options.JsonOptions)
                            ?? throw new ApiCodeException("non_default_api_result", "回應內容非 DefaultApiResult 格式無法解析");
                        ToLowerResult(ret);
                        if (ret.Result != "ok")
                            throw new ApiCodeException(ret.Result, ret.Message, ret.Data as JsonNode);
                    }
                    else
                    {
                        // t1 method()
                        var t1 = invocation.Method.ReturnType;
                        var resultT1 = typeof(DefaultApiResult<>).MakeGenericType([t1]);

                        var ret = JsonSerializer.Deserialize(s1, resultT1, _options.JsonOptions) as DefaultApiResult
                            ?? throw new ApiCodeException("non_default_api_result", "回應內容非 DefaultApiResult 格式無法解析");
                        ToLowerResult(ret);

                        // 兼容舊的沒有 X_Api_Result Header, 新的此值必定為OK
                        if (ret.Result != "ok")
                            throw new ApiCodeException(ret.Result, ret.Message, ret.Data as JsonNode);

                        var data = ret.Data;
                        step.Result = data;
                    }
                }
            }

            private void ToLowerResult(DefaultApiResult ret)
            {
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
                if (ret.Result != ret.Result.ToLower())
                    ret.Result = ret.Result.ToLower();
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
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

        public class JsonLowerCaseEnumFactory : JsonConverterFactory
        {
            public override bool CanConvert(Type typeToConvert)
            {
                return typeToConvert.IsEnum;
            }

            public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            {
                var converterType = typeof(JsonLowerCaseEnumConverter<>).MakeGenericType(typeToConvert);
                return Activator.CreateInstance(converterType) as JsonConverter ?? throw new NotSupportedException(typeToConvert.FullName);
            }
        }

        public class JsonLowerCaseEnumConverter<T> : JsonConverter<T> where T : struct, Enum
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
