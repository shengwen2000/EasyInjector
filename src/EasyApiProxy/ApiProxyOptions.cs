﻿using Castle.DynamicProxy;
using EasyApiProxys.Options;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;

namespace EasyApiProxys
{
    /// <summary>
    /// Api Proxy 建立選項
    /// </summary>
    public class ApiProxyOptions
    {
        /// <summary>
        /// 執行逾時 Default 15秒
        /// </summary>
        public TimeSpan DefaultTimeout { get; set; }

        /// <summary>
        /// 基礎連結位置
        /// </summary>
        public string BaseUrl { get; set; }              

        /// <summary>
        /// 取得Message Handler (default HttpClientHandler)
        /// </summary>
        public Func<HttpMessageHandler> GetHttpMessageHandler { get; set; }

        /// <summary>
        /// 取得Json Serializer
        /// </summary>
        public Func<JsonSerializer> GetJsonSerializer { get; set; }

        /// <summary>
        /// (1)產生HttpRequest之前
        /// </summary>
        public Func<StepContext, Task> Step1 { get; set; }

        /// <summary>
        /// (2)準備送出HttpRequest之前
        /// </summary>
        public Func<StepContext, Task> Step2 { get; set; }

        /// <summary>
        /// (3)當取得HttpResponse時
        /// </summary>
        public Func<StepContext, Task> Step3 { get; set; }

        /// <summary>
        /// (4)要回傳內容時
        /// </summary>
        public Func<StepContext, Task> Step4 { get; set; }

        JsonSerializer _serializer = new JsonSerializer();

        /// <summary>
        /// Api Proxy 建立選項
        /// </summary>
        public ApiProxyOptions()
        {
            DefaultTimeout = TimeSpan.FromSeconds(15);
            GetHttpMessageHandler = () => null;           
            GetJsonSerializer = () => _serializer;
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
            public IInvocation Invocation { get; set; }

            /// <summary>
            /// ApiProxy建立選項
            /// </summary>
            public ApiProxyOptions Options { get; set; }

            /// <summary>
            /// Request (2)準備送出HttpRequest之前
            /// </summary>
            public HttpRequestMessage Request { get; set; }

            /// <summary>
            /// Response (3)當取得HttpResponse時
            /// </summary>
            public HttpResponseMessage Response { get; set; }

            /// <summary>
            /// 回傳值 (3)當取得HttpResponse時
            /// </summary>
            public object Result { get; set; }

            /// <summary>
            /// 其他數據
            /// </summary>
            public Hashtable Items { get; set; }
        }        
    }
}


