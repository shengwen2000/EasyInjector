using EasyApiProxys.Options;
using System.Collections;
using System.Reflection;

namespace EasyApiProxys
{
    /// <summary>
    /// 負責實際呼叫Api的實作
    /// </summary>
    /// <typeparam name="TAPI"></typeparam>
    public class ApiProxyInterceptor<TAPI> : DispatchProxy, IDisposable
    {
        /// <summary>
        /// Builder Options
        /// </summary>
        public ApiProxyBuilderOptions BuildOptions { get; set; } = default!;

        /// <summary>
        /// 實例變數
        /// - 如果是針對單一實例的要求，而不是針對全體實例的要求可以放置在這裡
        /// - 內容如果有 IDisposable 會於 Dispose時 一併銷毀
        /// </summary>
        public Hashtable InstanceItems { get; set; } = [];

        /// <summary>
        /// 共用的 httpClient
        /// </summary>
        public HttpClient Http { get; set; } = default!;

        private bool disposed = false;

        /// <summary>
        /// dispose
        /// </summary>
        ~ApiProxyInterceptor()
        {
            Dispose(false);
        }

        /// <summary>
        /// /// 被呼叫的方法與參數
        /// </summary>
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            try
            {
                if (targetMethod == null) return null;
                if (args == null) return null;

                var invocation = new Invocation(targetMethod, args);

                // async method void return
                if (invocation.Method.ReturnType == typeof(Task))
                {
                    var ret = CallWebApi(invocation);
                    return ret;
                }
                // asyn method has return value
                else if (invocation.Method.ReturnType.IsGenericType && invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var ret = CallWebApi(invocation);
                    // Task<Type1>
                    var type1 = invocation.Method.ReturnType.GetGenericArguments().First();

                    // Task<Object> to Task<Type1>
                    var ret2 = typeof(ApiProxyInterceptor<TAPI>).GetMethod(nameof(ToTask), BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod([type1])
                        .Invoke(this, [ret]);
                    return ret2;
                }
                // 非Async Method
                else
                {
                    var ret = CallWebApi(invocation).GetAwaiter().GetResult();
                    return ret;
                }
            }
            catch (TargetInvocationException ex)
            {
                // 重新拋出內部的原始異常
                throw ex.InnerException ?? ex;
            }
        }

        /// <summary>
        /// 轉換Task回傳類型 (轉換Task回傳類型)
        /// </summary>
        static Task<T> ToTask<T>(Task<object> a)
        {
            return a.ContinueWith<T>(x =>
            {
                if (x.IsFaulted)
                {
                    if (x.Exception?.InnerException != null)
                        throw x.Exception.InnerException;
                    throw x.Exception!;
                }
                return (T)x.Result;
            }, TaskContinuationOptions.NotOnCanceled);
        }

        /// <summary>
        /// 實際呼叫WebApi
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns>回傳內容</returns>
        private async Task<object?> CallWebApi(Invocation invocation)
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

            using var req = new HttpRequestMessage();
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
            using var resp = await Http.SendAsync(req, cts.Token).ConfigureAwait(false);
            stepContext.Response = resp;

            if (BuildOptions.Step3 != null)
                await BuildOptions.Step3(stepContext).ConfigureAwait(false);

            if (BuildOptions.Step4 == null)
                return stepContext.Result;

            await BuildOptions.Step4(stepContext).ConfigureAwait(false);
            return stepContext.Result;
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
                    try {(val1 as IDisposable)?.Dispose(); }
                    catch {}
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
