using EasyApiProxys.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
        /// Default Api 預設 Json Serializer
        /// </summary>
        public static JsonSerializer DefaultJsonSerializer { get; private set; }

        /// <summary>
        /// Default Api 預設 JsonSerializerSettings
        /// 驼峰命名 日期(無時區與毫秒) 2024-08-06T15:18:41 Enum 字串
        /// </summary>
        public static JsonSerializerSettings DefaultJsonSerializerSettings { get; private set; }

        static DefaultApiExtension()
        {
            DefaultJsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Unspecified,
                //ContractResolver = new DefaultContractResolver()
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            var dateConverter = new Newtonsoft.Json.Converters.IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss"
            };
            DefaultJsonSerializerSettings.Converters.Add(dateConverter);
            DefaultJsonSerializerSettings.Converters.Add(new StringEnumConverter());

            DefaultJsonSerializer = JsonSerializer.Create(DefaultJsonSerializerSettings);
        }

        /// <summary>
        /// 使用標準Api通訊方法
        /// </summary>
        /// <param name="baseUrl">共用的HttpClient KeyName</param>
        /// <param name="builder">builder</param>
        /// <param name="defaltTimeoutSeconds">預設逾時秒數</param>
        static public ApiProxyBuilder UseDefaultApiProtocol(this ApiProxyBuilder builder, string baseUrl, int defaltTimeoutSeconds = 15)
        {
            var handler = new DefaultApiHandler(builder.Options.Step2, builder.Options.Step3);
            builder.Options.Handlers.Add(handler);

            builder.Options.GetJsonSerializer = () => DefaultJsonSerializer;
            builder.Options.Step2 = handler.Step2;
            builder.Options.Step3 = handler.Step3;
            builder.Options.BaseUrl = baseUrl;
            builder.Options.DefaultTimeout = TimeSpan.FromSeconds(defaltTimeoutSeconds);

            return builder;
        }

        internal class DefaultApiHandler
        {
            private const string RESULT_OK = "OK";
            private const string RESULT_IM = "IM";
            private const string RESULT_NON_DEFAULT_API_RESULT = "NON_DEFAULT_API_RESULT";

            Func<StepContext, Task> _step2;
            Func<StepContext, Task> _step3;

            public DefaultApiHandler(
                Func<StepContext, Task> step2,
                Func<StepContext, Task> step3)
            {
                _step2 = step2;
                _step3 = step3;
            }

            public async Task Step2(StepContext step)
            {
                if (_step2 != null)
                    await _step2(step).ConfigureAwait(false);

                var req = step.Request;
                var _options = step.BuilderOptions;

                req.Method = HttpMethod.Post;

                // 方法的第一個參數 會當成Json內容進行傳送
                if (step.Invocation.Arguments.Any())
                {
                    using (var sw = new StringWriter())
                    {
                        _options.GetJsonSerializer().Serialize(sw, step.Invocation.Arguments[0]);
                        req.Content = new StringContent(sw.ToString(), Encoding.UTF8, "application/json");
                    }
                }
            }

            public async Task Step3(StepContext step)
            {
                if (_step3 != null)
                    await _step3(step).ConfigureAwait(false);

                var resp = step.Response;
                var invocation = step.Invocation;
                var _options = step.BuilderOptions;

                // NOT OK 拋送 HttpRequestException
                resp.EnsureSuccessStatusCode();

                // 標題含有Result資訊 預先載入
                var resultCode = resp.Headers.GetValues(HeaderName_Result).SingleOrDefault() ?? RESULT_OK;

                // 取得回應內容
                var s1 = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using (var sr = new StreamReader(s1))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    // 方法沒有回傳值(void | task)
                    if (invocation.Method.ReturnType == typeof(void) || invocation.Method.ReturnType == typeof(Task))
                    {
                        // ok
                        if (resultCode.Equals(RESULT_OK, StringComparison.OrdinalIgnoreCase))
                            return;

                        // im
                        if (resultCode.Equals(RESULT_IM, StringComparison.OrdinalIgnoreCase))
                        {
                            var ret = _options.GetJsonSerializer().Deserialize<DefaultApiResult<JToken>>(jsonTextReader);
                            if (ret == null)
                                throw new ApiCodeException(RESULT_NON_DEFAULT_API_RESULT, "回應內容非 DefaultApiResult 格式無法解析");
                            throw new ApiCodeException(ret.Result, ret.Message, ret.Data);
                        }
                        // not ok
                        {
                            var ret = _options.GetJsonSerializer().Deserialize<DefaultApiResult<object>>(jsonTextReader);
                            if (ret == null)
                                throw new ApiCodeException(RESULT_NON_DEFAULT_API_RESULT, "回應內容非 DefaultApiResult 格式無法解析");
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
                            var ret = _options.GetJsonSerializer().Deserialize(jsonTextReader, dataType);
                            step.Result = ret;
                            return;
                        }

                        // im
                        if (resultCode.Equals(RESULT_IM, StringComparison.OrdinalIgnoreCase))
                        {
                            var ret = _options.GetJsonSerializer().Deserialize<DefaultApiResult<JToken>>(jsonTextReader);
                            if (ret == null)
                                throw new ApiCodeException(RESULT_NON_DEFAULT_API_RESULT, "回應內容非 DefaultApiResult 格式無法解析");
                            throw new ApiCodeException(ret.Result, ret.Message, ret.Data);
                        }
                        // not ok
                        else
                        {
                            var ret = _options.GetJsonSerializer().Deserialize<DefaultApiResult<object>>(jsonTextReader);
                            if (ret == null)
                                throw new ApiCodeException(RESULT_NON_DEFAULT_API_RESULT, "回應內容非 DefaultApiResult 格式無法解析");
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
                            var ret = _options.GetJsonSerializer().Deserialize(jsonTextReader, dataType);
                            step.Result = ret;
                            return;
                        }
                        // im
                        if (resultCode.Equals(RESULT_IM, StringComparison.OrdinalIgnoreCase))
                        {
                            var ret = _options.GetJsonSerializer().Deserialize<DefaultApiResult<JToken>>(jsonTextReader);
                            if (ret == null)
                                throw new ApiCodeException(RESULT_NON_DEFAULT_API_RESULT, "回應內容非 DefaultApiResult 格式無法解析");
                            throw new ApiCodeException(ret.Result, ret.Message, ret.Data);
                        }
                        // not ok
                        {
                            var ret = _options.GetJsonSerializer().Deserialize<DefaultApiResult<object>>(jsonTextReader);
                            if (ret == null)
                                throw new ApiCodeException(RESULT_NON_DEFAULT_API_RESULT, "回應內容非 DefaultApiResult 格式無法解析");
                            throw new ApiCodeException(ret.Result, ret.Message, ret.Data);
                        }
                    }
                }
            }
        }
    }
}
