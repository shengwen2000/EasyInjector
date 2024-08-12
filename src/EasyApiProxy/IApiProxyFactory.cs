﻿using Castle.DynamicProxy;
using System;
using System.Collections;
using System.Net.Http;

namespace EasyApiProxys
{
    /// <summary>
    /// 建立Proxy實例
    /// </summary>
    public interface IApiProxyFactory<TAPI> where TAPI : class
    {
        /// <summary>
        /// 建立Proxy實例
        /// </summary>
        ApiProxy<TAPI> Create();
    }

    /// <summary>
    /// 建立Proxy實例
    /// </summary>
    public class ApiProxyFactory<TAPI> : IApiProxyFactory<TAPI> where TAPI : class
    {
        private readonly ApiProxyBuilderOptions _options;
        private readonly HttpClient _http;

        /// <summary>
        /// 建立Proxy實例
        /// </summary>
        public ApiProxyFactory(ApiProxyBuilderOptions options)
        {
            _options = options;

            var handler = options.GetHttpMessageHandler();
            if (handler != null)
                _http = new HttpClient(handler);
            else
                _http = new HttpClient();
            _http.Timeout = options.DefaultTimeout;     
        }

        /// <summary>
        /// 建立實例
        /// </summary>
        public ApiProxy<TAPI> Create()
        {
            var instopt = new Hashtable();

            var proxy = new ApiProxy<TAPI>();
            proxy.Items = instopt;

            var generator = new ProxyGenerator();
            var inteceptor1 = new ApiProxyInterceptor<TAPI>(_http, _options, proxy.Items);
            var api = generator.CreateInterfaceProxyWithoutTarget<TAPI>(inteceptor1);
            proxy.Api = api;
            return proxy;
        }

    }
}