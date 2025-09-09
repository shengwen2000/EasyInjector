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
        /// Factory
        /// - singleton 每一種Proxy 都有單一Factory
        /// </summary>
        IApiProxyFactory<TAPI> Factory { get; }

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
        /// 指定 AuthorizationHeader
        /// </summary>
        void SetAuthorization(string headerValue);

        /// <summary>
        /// 指定 AuthorizationHeader Provider
        /// </summary>
        void SetAuthorizationProvider(Func<string> getHeaderValue);

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

        /// <summary>
        /// Factory
        /// </summary>
        public IApiProxyFactory<TAPI> Factory { get; private set; }

        public Action<StepContext>? BeforeHttpPost { get; set; }

        public Action<StepContext>? AfterHttpPost { get; set; }

        /// <summary>
        /// 指定Auth Header
        /// </summary>
        Func<string?>? _getAuthHeader;

        /// <summary>
        /// Api 代理物件
        /// </summary>
        public ApiProxy(ApiProxyInterceptor<TAPI> interceptor, TAPI api, IApiProxyFactory<TAPI> factory)
        {
            _api = api;
            _interceptor = interceptor;
            _interceptor.InstanceItems["InstanceCall_Step2"] = new Func<StepContext, Task>(Step2InstanceCall);
            _interceptor.InstanceItems["InstanceCall_Step3"] = new Func<StepContext, Task>(Step3InstanceCall);
            Factory = factory;
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
            _getAuthHeader = () => "Bearer " + getToken();
        }

        /// <summary>
        /// 指定 Auth Header
        /// </summary>
        /// <param name="headerValue">Authorization Header Value</param>
        public void SetAuthorization(string headerValue)
        {
            SetAuthorizationProvider(() => headerValue);
        }

        /// <summary>
        /// 指定 Auth Header Provider
        /// </summary>
        /// <param name="getHeaderValue">Authorization Header Value Provider</param>
        public void SetAuthorizationProvider(Func<string> getHeaderValue)
        {
            _getAuthHeader = getHeaderValue;
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
            if (_getAuthHeader != null)
            {
                var headerValue = _getAuthHeader();
                if (headerValue != null && string.IsNullOrEmpty(headerValue) == false)
                    context.Request!.Headers.Authorization = AuthenticationHeaderValue.Parse(headerValue);
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
