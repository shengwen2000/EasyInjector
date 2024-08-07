﻿using Castle.DynamicProxy;
using EasyApiProxys.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EasyApiProxys
{
    /// <summary>
    /// 負責實際呼叫Api的實作
    /// </summary>
    /// <typeparam name="TAPI"></typeparam>
    internal class ApiProxyInterceptor<TAPI> : IInterceptor
    {
        private readonly ApiProxyOptions _options;

        /// <summary>
        /// 共用的 httpClient
        /// </summary>
        private readonly HttpClient _http;

        /// <summary>
        /// 負責實際呼叫Api的實作
        /// </summary>
        /// <param name="options">代理選項</param>
        public ApiProxyInterceptor(ApiProxyOptions options)
        {
            _options = options;
            var handler = _options.GetHttpMessageHandler();
            if (handler != null)
                _http = new HttpClient(handler);
            else
                _http = new HttpClient();
            _http.Timeout = _options.DefaultTimeout;           
        }

        /// <summary>
        /// 攔截介面方法進入點
        /// </summary>
        /// <param name="invocation">那個呼叫方法</param>
        public void Intercept(IInvocation invocation)
        {
            // async method void return
            if (invocation.Method.ReturnType == typeof(Task))
            {
                var ret = CallWebApi(invocation);
                invocation.ReturnValue = CallWebApi(invocation);
                return;
            }
            // asyn method has return value
            else if (invocation.Method.ReturnType.IsGenericType && invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var ret = CallWebApi(invocation);
                // Task<Type1>
                var type1 = invocation.Method.ReturnType.GetGenericArguments().First();

                // Task<Object> to Task<Type1>
                var ret2 = GetType().GetMethod("ToTask", System.Reflection.BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(new[] { type1 })
                    .Invoke(this, new[] { ret });

                invocation.ReturnValue = ret2;
                return;
            }
            // 非Async Method
            else
            {               
                var ret = CallWebApi(invocation).GetAwaiter().GetResult();
                invocation.ReturnValue = ret;
                return;
            }
        }

        // 不可以變更名稱
        // 轉換Task回傳類型
        Task<T> ToTask<T>(Task<object> a)
        {
            return a.ContinueWith<T>(x =>
            {
                if (x.IsFaulted)
                    throw x.Exception.InnerException;
                return (T)x.Result;
            }, TaskContinuationOptions.NotOnCanceled);
        }

        /// <summary>
        /// 實際呼叫WebApi
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns>回傳內容</returns>
        private async Task<object> CallWebApi(IInvocation invocation)
        {
            var stepContext = new StepContext { 
                Invocation = invocation,
                Options = _options            
            };
            

            // step1
            if (_options.Step1 != null)
                await _options.Step1(stepContext).ConfigureAwait(false);

            // 呼叫哪個Api
            var apiMethod = invocation.Method;
            var apiAttr = apiMethod.GetCustomAttribute<ApiMethodAttribute>();

            using (var req = new HttpRequestMessage())
            {
                // API Url e.g. http://demo/demoapi
                if (apiAttr != null && !string.IsNullOrEmpty(apiAttr.Name))
                    req.RequestUri = new Uri(string.Concat(_options.BaseUrl, "/", apiAttr.Name));
                else
                    req.RequestUri = new Uri(string.Concat(_options.BaseUrl, "/", apiMethod.Name));
                stepContext.Request = req;
                if (_options.Step2 != null)
                    await _options.Step2(stepContext).ConfigureAwait(false);

                // API逾時設定
                var cts = new CancellationTokenSource();
                if (apiAttr != null && apiAttr.TimeoutSeconds > 0)
                    cts.CancelAfter(TimeSpan.FromSeconds(apiAttr.TimeoutSeconds));
                else
                    cts.CancelAfter(_options.DefaultTimeout);

                // 進行呼叫
                using (var resp = await _http.SendAsync(req, cts.Token).ConfigureAwait(false))
                {
                    stepContext.Response = resp;                    

                    if (_options.Step3 != null)
                        await _options.Step3(stepContext).ConfigureAwait(false);

                    if (_options.Step4 == null)
                        return stepContext.Result;

                    await _options.Step4(stepContext).ConfigureAwait(false);
                    return stepContext.Result;
                }
            }
        }
    }
}
