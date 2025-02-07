using EasyApiProxys.Options;
using System.Text.Json;
using System.Collections;

namespace EasyApiProxys
{
    /// <summary>
    /// Api Proxy 建立選項
    /// </summary>
    public class ApiProxyBuilderOptions
    {
        /// <summary>
        /// 執行逾時 Default 15秒
        /// </summary>
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(15);

        /// <summary>
        /// 基礎連結位置
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// 取得Message Handler (default HttpClientHandler)
        /// </summary>
        public Func<HttpMessageHandler> GetHttpMessageHandler { get; set; }

        /// <summary>
        /// Json Serializer Options
        /// (default is JsonSerializerDefaults.Web)
        /// </summary>
        public JsonSerializerOptions JsonOptions { get; set; } = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        /// <summary>
        /// (1)產生HttpRequest之前
        /// </summary>
        public Func<StepContext, Task>? Step1 { get; set; }

        /// <summary>
        /// (2)準備送出HttpRequest之前
        /// </summary>
        public Func<StepContext, Task>? Step2 { get; set; }

        /// <summary>
        /// (3)當取得HttpResponse時
        /// </summary>
        public Func<StepContext, Task>? Step3 { get; set; }

        /// <summary>
        /// (4)要回傳內容時
        /// </summary>
        public Func<StepContext, Task>? Step4 { get; set; }

        /// <summary>
        /// Request Handler 註冊後會放在這裡
        /// - Proxy Factory Dispose 時 會一併釋放
        /// </summary>
        public List<object> Handlers { get; set; } = [];

        /// <summary>
        /// Api Proxy 建立選項
        /// </summary>
        public ApiProxyBuilderOptions()
        {
            GetHttpMessageHandler = () => new HttpClientHandler();
        }
    }

    namespace Options
    {
        /// <summary>
        /// 步驟Context
        /// </summary>
        public class StepContext
        {
            /// <summary>
            /// 呼叫方法
            /// </summary>
            public Invocation Invocation { get; set; } = default!;

            /// <summary>
            /// Buidler Options (所有的Proxy同一份)
            /// </summary>
            public ApiProxyBuilderOptions BuilderOptions { get; set; } = default!;

            /// <summary>
            /// 實例變數
            /// - 如果是針對單一實例的要求，而不是針對全體實例的要求可以放置在這裡
            /// </summary>
            public Hashtable InstanceItems { get; set; } = default!;

            /// <summary>
            /// Request (2)準備送出HttpRequest之前
            /// </summary>
            public HttpRequestMessage? Request { get; set; }

            /// <summary>
            /// Response (3)當取得HttpResponse時
            /// </summary>
            public HttpResponseMessage? Response { get; set; }

            /// <summary>
            /// 回傳值 (3)當取得HttpResponse時
            /// </summary>
            public object? Result { get; set; }
        }
    }
}


