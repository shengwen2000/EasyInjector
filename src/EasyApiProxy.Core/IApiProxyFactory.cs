using System.Reflection;

namespace EasyApiProxys
{
    /// <summary>
    /// 建立Proxy實例
    /// </summary>
    public interface IApiProxyFactory<TAPI> : IDisposable
        where TAPI : class
    {
        /// <summary>
        /// 建立Proxy實例
        /// </summary>
        IApiProxy<TAPI> Create();
    }

    /// <summary>
    /// 建立Proxy實例
    /// </summary>
    public class ApiProxyFactory<TAPI> : IApiProxyFactory<TAPI> where TAPI : class
    {
        private readonly ApiProxyBuilderOptions _options;
        private readonly HttpClient _http;
        private bool disposed = false;

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
        public IApiProxy<TAPI> Create()
        {
            var api = DispatchProxy.Create<TAPI, ApiProxyInterceptor<TAPI>>();

            var inteceptor1 =  api as ApiProxyInterceptor<TAPI> ?? throw new Exception("convert faild");
            inteceptor1.Http = _http;
            inteceptor1.BuildOptions = _options;

            var proxy = new ApiProxy<TAPI>(inteceptor1, api);
            return proxy;
        }

         /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
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
                if (_options.Handlers != null)
                {
                    foreach (var handler1 in _options.Handlers)
                    {
                        try { (handler1 as IDisposable)?.Dispose(); }
                        catch {}
                    }
                    _options.Handlers.Clear();
                }
            }
            //不正常Dispose只要確保自身資源釋放即可
            else
            {
            }
        }

    }
}
