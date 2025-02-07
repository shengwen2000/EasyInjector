using EasyInjectors;

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
        /// The Builder 建立 Api Proxy Factory
        /// </summary>
        public ApiProxyBuilder()
        {
            Options = new ApiProxyBuilderOptions();
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
        /// <param name="easyInjector">如果要註冊到EasyInjector的話傳入此參數</param>
        public IApiProxyFactory<TAPI> Build<TAPI>(EasyInjector easyInjector = null) where TAPI : class
        {
            // 預設最後套用 InstanceCall Handler
            InstanceCallExtension.UseInstanceCallHandler(this);

            var factory = new ApiProxyFactory<TAPI>(Options);

            // 需要整合注入依賴的話
            if (easyInjector != null)
                RegisterService(easyInjector, factory);

            return factory;
        }

        /// <summary>
        /// 整合EasyInjector注入依賴的話
        /// </summary>
        private static void RegisterService<TAPI>(EasyInjector injector, ApiProxyFactory<TAPI> factory) where TAPI : class
        {
            injector.AddSingleton<IApiProxyFactory<TAPI>>(sp => factory);
            injector.AddScoped<IApiProxy<TAPI>>(sp => sp.GetRequiredService<IApiProxyFactory<TAPI>>().Create());
            injector.AddScoped<TAPI>(sp => sp.GetRequiredService<IApiProxy<TAPI>>().Api);
        }
    }
}
