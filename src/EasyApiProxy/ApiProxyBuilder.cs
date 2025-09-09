
using System;
namespace EasyApiProxys
{
    /// <summary>
    /// The Builder 建立 Api Proxy Factory
    /// </summary>
    public class ApiProxyBuilder
    {
        /// <summary>
        /// Proxy建立選項
        /// </summary>
        public ApiProxyBuilderOptions Options { get; set; }

        /// <summary>
        /// 直接建立代理服務實例
        /// </summary>
        private Func<IServiceProvider, object> _createLocalInstance;

        /// <summary>
        /// The Builder 建立 Api Proxy Factory
        /// </summary>
        public ApiProxyBuilder()
        {
            Options = new ApiProxyBuilderOptions();
        }

        /// <summary>
        /// 直接建立本地服務實例
        /// - 情境:如果服務可以是遠端或是本地提供時，如果選則為本地 則可以使用此方法。
        /// </summary>        
        public ApiProxyBuilder UseLocalApi<TAPI>(Func<IServiceProvider, TAPI> createInstacne) where TAPI : class
        {
            _createLocalInstance = createInstacne;
            return this;
        }

        /// <summary>
        /// 建立的 FACTORY 應該要重複使用，而不是只用一次就不用
        /// 因為每個FACTORY 都綁定一個新的HTTPClient 所建立的 Proxy 都共用同一份 HttpClient實例
        /// 若每次請求建立新的 Facotry，重度使用下可能用光 Socket Port 導致 SocketException。
        /// 
        /// 如果要註冊到EasyInjector的話傳入此參數easyInjector
        /// - 可以取得Singleton服務 IApiProxyFactory(TAPI)
        /// - 可以取得Scope服務 IApiProxy(TAPI)
        /// - 可以取得Scope服務 TAPI
        /// </summary>      
        public IApiProxyFactory<TAPI> Build<TAPI>() where TAPI : class
        {
            // 本地服務(就是不透過遠端)
            if (_createLocalInstance != null)
            {
                var factory = new ApiProxyFactoryLocal<TAPI>(_createLocalInstance);
                return factory;
            }
            else
            {
                // 預設最後套用 InstanceCall Handler
                InstanceCallExtension.UseInstanceCallHandler(this);

                var factory = new ApiProxyFactory<TAPI>(Options);
                return factory;
            }
        }

        /// <summary>
        /// (直接建立本地服務實例) 專用 Factory
        /// </summary>
        /// <typeparam name="TAPI"></typeparam>
        internal class ApiProxyFactoryLocal<TAPI> : IApiProxyFactory<TAPI>
            where TAPI : class
        {
            private readonly Func<IServiceProvider, object> _createInstacne;

            public ApiProxyFactoryLocal(Func<IServiceProvider, object> createInstacne)
            {
                _createInstacne = createInstacne;
            }           

            public void Dispose()
            {                
            }

            public IApiProxy<TAPI> Create(IServiceProvider sp)
            {
                var api = _createInstacne(sp);
                if (api == null)
                    throw new ApplicationException(string.Format("建立服務實例{0}回傳空值", typeof(TAPI).Name));

                var tapi = api as TAPI;
                if (tapi == null)
                    throw new ApplicationException(string.Format("建立服務實例{0}回傳空值", typeof(TAPI).Name));

                return new ApiProxyLocal<TAPI>(tapi, this);
            }

            public ApiProxyBuilderOptions Options
            {
                get { return null; }
            }
        }

        /// <summary>
        /// (直接建立本地服務實例) 專用 ApiProxy
        /// </summary>
        internal class ApiProxyLocal<TAPI> : IApiProxy<TAPI>
            where TAPI : class
        {
            private readonly TAPI _api;

            /// <summary>
            /// Factory
            /// </summary>
            public IApiProxyFactory<TAPI> Factory { get; private set; }

            public ApiProxyLocal(TAPI api, IApiProxyFactory<TAPI> factory)
            {
                _api = api;
                Factory = factory;
            }

            public TAPI Api { get { return _api; } }

            public void SetBearer(string bearerToken)
            {                
            }

            public void SetBearerProvider(Func<string> getBearerToken)
            {
            }

            public void SetAuthorization(string headerValue)
            {               
            }

            public void SetAuthorizationProvider(Func<string> getHeaderValue)
            {             
            }

            public Action<Options.StepContext> BeforeHttpPost { get; set; }

            public Action<Options.StepContext> AfterHttpPost { get; set; }

            public void Dispose()
            {              
            }
            
        }
    }
}
