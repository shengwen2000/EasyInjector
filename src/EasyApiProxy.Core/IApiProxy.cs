using EasyApiProxys.Options;
using System.Net.Http.Headers;

namespace EasyApiProxys
{
    /// <summary>
    /// Api 代理物件
    /// - 提供代理物件
    /// - 可以設定BearToken
    /// - 提供 Http呼叫的攔截
    /// </summary>
    public interface IApiProxy<TAPI> : IDisposable
        where TAPI : class
    {
        /// <summary>
        /// API的代理物件
        /// </summary>
        TAPI Api { get; }

        /// <summary>
        /// 指定 BearerToken
        /// </summary>
        void SetBearer(string? bearerToken);

        /// <summary>
        /// 指定 BearerToken Provider
        /// </summary>
        void SetBearerProvider(Func<string?> getBearerToken);

        /// <summary>
        /// Http Post 前攔截事件
        /// </summary>
        Action<StepContext>? BeforeHttpPost { get; set; }

        /// <summary>
        /// Http Post 後攔截事件
        /// </summary>
        Action<StepContext>? AfterHttpPost { get; set; }
    }

    /// <summary>
    /// Api 代理物件
    /// </summary>
    public class ApiProxy<TAPI> : IApiProxy<TAPI>
        where TAPI : class
    {
        private readonly TAPI _api;
        private readonly ApiProxyInterceptor<TAPI> _interceptor;
        private bool disposed = false;

        public Action<StepContext>? BeforeHttpPost { get; set; }

        public Action<StepContext>? AfterHttpPost { get; set; }

        Func<string?>? _getBearerToken;

        /// <summary>
        /// Api 代理物件
        /// </summary>
        public ApiProxy(ApiProxyInterceptor<TAPI> interceptor, TAPI api)
        {
            _api = api;
            _interceptor = interceptor;
            _interceptor.InstanceItems["InstanceCall_Step2"] = new Func<StepContext, Task>(Step2InstanceCall);
            _interceptor.InstanceItems["InstanceCall_Step3"] = new Func<StepContext, Task>(Step3InstanceCall);
        }

        /// <summary>
        /// 解構
        /// </summary>
        ~ApiProxy()
        {
            Dispose(false);
        }

        /// <summary>
        ///
        /// </summary>
        public TAPI Api
        {
            get
            {
                return _api;
            }
        }

        /// <summary>
        /// 指定 BearerToken
        /// </summary>
        /// <param name="token"></param>
        public void SetBearer(string? token)
        {
            SetBearerProvider(() => token);
        }

        /// <summary>
        /// 指定 BearerToken Provider
        /// </summary>
        /// <param name="getToken"></param>
        public void SetBearerProvider(Func<string?> getToken)
        {
            _getBearerToken = getToken;
        }

        /// <summary>
        /// 解構
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        async Task Step2InstanceCall(StepContext context)
        {
            await Task.CompletedTask;
            if (_getBearerToken != null)
            {
                var token = _getBearerToken();
                if (token != null && string.IsNullOrEmpty(token) == false)
                    context.Request!.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            BeforeHttpPost?.Invoke(context);
        }

        async Task Step3InstanceCall(StepContext context)
        {
            await Task.CompletedTask;
            AfterHttpPost?.Invoke(context);
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
                _interceptor.Dispose();
            }
            //不正常Dispose只要確保自身資源釋放即可
            else
            {
            }
        }
    }
}
