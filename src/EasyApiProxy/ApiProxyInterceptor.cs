using Castle.DynamicProxy;
using EasyApiProxys.Options;
using System;
using System.Collections;
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
    public class ApiProxyInterceptor<TAPI> : IInterceptor, IDisposable
    {
        /// <summary>
        /// 建立選項
        /// </summary>
        public ApiProxyBuilderOptions BuildOptions { get; set; }

        /// <summary>
        /// 實例變數
        /// - 如果是針對單一實例的要求，而不是針對全體實例的要求可以放置在這裡
        /// - 內容如果有 IDisposable 會於 Dispose時 一併銷毀
        /// </summary>
        public Hashtable InstanceItems { get; set; }

        /// <summary>
        /// 共用的 httpClient
        /// </summary>
        public HttpClient Http { get; set; }

        private bool disposed = false;

        /// <summary>
        /// 負責實際呼叫Api的實作
        /// </summary>
        /// <param name="http">HttpClient</param>
        /// <param name="options">代理Builder選項</param>
        public ApiProxyInterceptor(HttpClient http, ApiProxyBuilderOptions options)
        {
            BuildOptions = options;
            InstanceItems = new Hashtable();
            Http = http;
        }

        /// <summary>
        /// dispose
        /// </summary>
        ~ApiProxyInterceptor()
        {
            Dispose(false);
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
                invocation.ReturnValue = ret;
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
            var stepContext = new StepContext
            {
                Invocation = invocation,
                BuilderOptions = BuildOptions,
                InstanceItems = InstanceItems
            };

            // step1
            if (BuildOptions.Step1 != null)
                await BuildOptions.Step1(stepContext).ConfigureAwait(false);

            // 呼叫哪個Api
            var apiMethod = invocation.Method;
            var apiAttr = apiMethod.GetCustomAttribute<ApiMethodAttribute>();

            using (var req = new HttpRequestMessage())
            {
                // API Url e.g. http://demo/demoapi
                if (apiAttr != null && !string.IsNullOrEmpty(apiAttr.Name))
                    req.RequestUri = new Uri(string.Concat(BuildOptions.BaseUrl, "/", apiAttr.Name));
                else
                    req.RequestUri = new Uri(string.Concat(BuildOptions.BaseUrl, "/", apiMethod.Name));
                stepContext.Request = req;
                if (BuildOptions.Step2 != null)
                    await BuildOptions.Step2(stepContext).ConfigureAwait(false);

                // API逾時設定
                var cts = new CancellationTokenSource();
                if (apiAttr != null && apiAttr.TimeoutSeconds > 0)
                    cts.CancelAfter(TimeSpan.FromSeconds(apiAttr.TimeoutSeconds));
                else
                    cts.CancelAfter(BuildOptions.DefaultTimeout);

                // 進行呼叫
                using (var resp = await Http.SendAsync(req, cts.Token).ConfigureAwait(false))
                {
                    stepContext.Response = resp;

                    if (BuildOptions.Step3 != null)
                        await BuildOptions.Step3(stepContext).ConfigureAwait(false);

                    if (BuildOptions.Step4 == null)
                        return stepContext.Result;

                    await BuildOptions.Step4(stepContext).ConfigureAwait(false);
                    return stepContext.Result;
                }
            }
        }

        /// <summary>
        /// dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// dispose
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            // 先說已經disposed 避免底下可能呼叫到內含自身服務的Dispose()形成無限迴圈
            disposed = true;

            //正常Dispose，所有子項目一併施放
            if (disposing)
            {
                foreach (var val1 in InstanceItems.Values)
                {
                    if (val1 != null && val1 is IDisposable)
                    {
                        try { (val1 as IDisposable).Dispose(); }
                        catch { }
                    }
                }
                InstanceItems.Clear();
            }
            //不正常Dispose只要確保自身資源釋放即可
            else
            {
            }
        }
    }
}
