using Castle.DynamicProxy;
using EasyApiProxys.Options;
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
                //var ret = Task.Run(() => CallWebApi(invocation)).GetAwaiter().GetResult();
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
            // step1
            if (_options.Step1 != null)
                await _options.Step1(new Step1_BeforeCreateRequest
                {
                    Invocation = invocation,
                    Options = _options
                });

            // 呼叫哪個Api
            var apiMethod = invocation.Method;

            using (var req = new HttpRequestMessage())
            {
                if (_options.Step2 != null)
                    await _options.Step2(new Step2_BeforeHttpSend
                    {
                        Invocation = invocation,
                        Options = _options,
                        Request = req
                    });

                // API逾時設定
                var cts = new CancellationTokenSource();
                cts.CancelAfter(_options.DefaultTimeout);

                // 進行呼叫
                using (var resp = await _http.SendAsync(req, cts.Token))
                {
                    var step3 = new Step3_AfterHttpResponse
                    {
                        Invocation = invocation,
                        Options = _options,
                        Response = resp
                    };

                    if (_options.Step3 != null)
                        await _options.Step3(step3);

                    if (_options.Step4 == null)
                        return step3.Result;

                    var step4 = new Step4_ReturnResult
                    {
                        Invocation = invocation,
                        Options = _options,
                        Result = step3.Result
                    };
                    await _options.Step4(step4);
                    return step4.Result;
                }
            }
        }
    }
}
