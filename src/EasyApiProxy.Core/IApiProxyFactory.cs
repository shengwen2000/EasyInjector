using System.Collections;
using System.Reflection;

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
        IApiProxy<TAPI> Create();
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
        public IApiProxy<TAPI> Create()
        {
            var instopt = new Hashtable();

            var proxy = new ApiProxy<TAPI>
            {
                Items = instopt
            };

            var api = DispatchProxy.Create<TAPI, ApiProxyInterceptor<TAPI>>();

            //var generator = new ProxyGenerator();
            var inteceptor1 =  api as ApiProxyInterceptor<TAPI> ?? throw new Exception("convert faild");
            inteceptor1.Http = _http;
            inteceptor1.BuildOptions = _options;
            inteceptor1.InstanceItems = proxy.Items;

            proxy.Object = api;
            return proxy;
        }

    }
}
