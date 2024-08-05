using EasyApiProxys.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Linq;
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
        /// Default Api 預設 Json Serializer
        /// </summary>
        public static JsonSerializer DefaultJsonSerializer { get; private set; }

        static DefaultApiExtension()
        {
            var setting = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Unspecified,
                //ContractResolver = new DefaultContractResolver()
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            DefaultJsonSerializer = JsonSerializer.Create(setting);
        }

        /// <summary>
        /// 使用標準Api通訊方法
        /// </summary>
        /// <param name="clientName">共用的HttpClient KeyName</param>
        /// <param name="builder"></param>
        static public ApiProxyBuilder UseDefaultApiProtocol(this ApiProxyBuilder builder, string baseUrl, string clientName="Default")
        {
            var hander = new DefaultApiHandler(builder.Options.Step2, builder.Options.Step3);
            if (clientName != null)
                builder.Options.ClientName = clientName;

            builder.Options.GetJsonSerializer = () => DefaultJsonSerializer;

            builder.Options.Step2 = hander.Step2;
            builder.Options.Step3 = hander.Step3;
            builder.Options.BaseUrl = baseUrl;
            return builder;
        }

        internal class DefaultApiHandler
        {
            Func<Step2_BeforeHttpSend,Task> _step2;
            Func<Step3_AfterHttpResponse,Task> _step3;

            public DefaultApiHandler(
                Func<Step2_BeforeHttpSend, Task> step2, 
                Func<Step3_AfterHttpResponse, Task> step3)
            {
                _step2 = step2;
                _step3 = step3;

            }

            public async Task Step2(Step2_BeforeHttpSend step)
            {
                if (_step2 != null)
                    await _step2(step);

                var apiMethod = step.Invocation.Method;
                var req = step.Request;
                var _options = step.Options;

                // API Url e.g. http://demo/demoapi
                req.RequestUri = new Uri(string.Concat(_options.BaseUrl, "/", apiMethod.Name));
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

            public async Task Step3(Step3_AfterHttpResponse step)
            {
                if (_step3 != null)
                    await _step3(step);

                var resp = step.Response;
                var invocation = step.Invocation;
                var _options = step.Options;

                // 必須是 HTTP 200 回應
                if (resp.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new ApiCodeException("HTTP_NOT_OK", string.Format("HTTP連線狀態錯誤 StatusCode={0}", resp.StatusCode));

                // 取得回應內容
                var s1 = await resp.Content.ReadAsStreamAsync();
                using (var sr = new StreamReader(s1))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    // 方法沒有回傳值(void | task)
                    if (invocation.Method.ReturnType == typeof(void) || invocation.Method.ReturnType == typeof(Task))
                    {
                        var ret = _options.GetJsonSerializer().Deserialize<DefaultApiResult>(jsonTextReader);
                        if (ret.Result != "OK")
                            throw new ApiCodeException(ret.Result, ret.Message);
                    }
                    // 方法有回傳值Task<T>
                    else if (invocation.Method.ReturnType.IsGenericType && invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        // Task<t1>
                        var t1 = invocation.Method.ReturnType.GetGenericArguments()[0];
                        var resultT1 = typeof(DefaultApiResult<>).MakeGenericType(new[] { t1 });

                        var ret = _options.GetJsonSerializer().Deserialize(jsonTextReader, resultT1) as DefaultApiResult;
                        if (ret.Result != "OK")
                            throw new ApiCodeException(ret.Result, ret.Message);

                        var data = ret.Data;
                        step.Result = data;
                    }
                    // 方法有回傳值(string Account ...)
                    else
                    {
                        // t1 method()
                        var t1 = invocation.Method.ReturnType;
                        var resultT1 = typeof(DefaultApiResult<>).MakeGenericType(new[] { t1 });

                        var ret = _options.GetJsonSerializer().Deserialize(jsonTextReader, resultT1) as DefaultApiResult;
                        if (ret.Result != "OK")
                            throw new ApiCodeException(ret.Result, ret.Message);

                        var data = ret.Data;
                        step.Result = data;
                    }
                }
            }
        }
    }
}
