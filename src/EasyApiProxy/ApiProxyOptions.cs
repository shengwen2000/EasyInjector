using Castle.DynamicProxy;
using EasyApiProxys;
using KmuApps.ApiProxys.Filters;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace KmuApps.ApiProxys
{
    public class ApiProxyOptions
    {
        /// <summary>
        /// Http Client Cached Name (default is Default)
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// 執行逾時 Default 15秒
        /// </summary>
        public TimeSpan DefaultTimeout { get; set; }

        /// <summary>
        /// 基礎連結位置
        /// </summary>
        public string BaseUrl { get; set; }        

        /// <summary>
        /// Http Client 快取服務 (有預設實作)
        /// </summary>
        public IHttpClientCacheService HttpClientCache { get; set; }

        /// <summary>
        /// 取得Message Handler (default null)
        /// </summary>
        public Func<HttpMessageHandler> GetHttpMessageHandler { get; set; }

        /// <summary>
        /// Json Serializer
        /// </summary>
        public Func<JsonSerializer> GetJsonSerializer { get; set; }

        /// <summary>
        /// (1)產生HttpRequest之前
        /// </summary>
        public Func<Step1_BeforeCreateRequest,Task> Step1 { get; set; }

        /// <summary>
        /// (2)準備送出HttpRequest之前
        /// </summary>
        public Func<Step2_BeforeHttpSend,Task> Step2 { get; set; }

        /// <summary>
        /// (3)當取得HttpResponse時
        /// </summary>
        public Func<Step3_AfterHttpResponse,Task> Step3 { get; set; }

        /// <summary>
        /// (4)要回傳內容時
        /// </summary>
        public Func<Step4_ReturnResult, Task> Step4 { get; set; }

        JsonSerializer _serializer = new JsonSerializer();

        public ApiProxyOptions()
        {
            ClientName = "Default";
            DefaultTimeout = TimeSpan.FromSeconds(15);
            GetHttpMessageHandler = () => null;
            HttpClientCache = HttpClientCacheService.Shared;
            GetJsonSerializer = () => _serializer;            
        }
    }

    namespace Filters
    {
        public class BaseStep
        {
            /// <summary>
            /// 呼叫方法
            /// </summary>
            public IInvocation Invocation { get; set; }

            public ApiProxyOptions Options { get; set; }
        }

        /// <summary>
        /// (1)產生HttpRequest之前
        /// </summary>
        public class Step1_BeforeCreateRequest : BaseStep
        {
           
        }

        /// <summary>
        /// (2)準備送出HttpRequest之前
        /// </summary>
        public class Step2_BeforeHttpSend : BaseStep
        {
            /// <summary>
            /// Request
            /// </summary>
            public HttpRequestMessage Request { get; set; }
        }        

        /// <summary>
        /// (3)當取得HttpResponse時
        /// </summary>
        public class Step3_AfterHttpResponse : BaseStep
        {
            /// <summary>
            /// Response
            /// </summary>
            public HttpResponseMessage Response { get; set; }

            /// <summary>
            /// 回傳值
            /// </summary>
            public object Result { get; set; }
        }

        /// <summary>
        /// (4)要回傳內容時
        /// </summary>
        public class Step4_ReturnResult : BaseStep
        {
            /// <summary>
            /// 回傳值
            /// </summary>
            public object Result { get; set; }
        }
    }
}


