
using EasyInjectors;
using System;

namespace EasyApiProxys
{

    /// <summary>
    /// 注入 ApiProxy
    /// </summary>
    public static class SetupExtensions
    {      
        /// <summary>
        /// 加入 ApiProxy 註冊到 easyInjector 注入依賴服務內
        /// - 可以取得 Singleton 服務 IApiProxyFactory of TService
        /// - 可以取得 Scoped 服務 IApiProxy of TService
        /// - 可以取得 Scoped 服務 TService
        /// </summary>
        /// <typeparam name="TService">服務API類別</typeparam>
        /// <param name="injector">EasyInjector Instance</param>
        /// <param name="configApiAction">規劃API參數</param>        
        static public EasyInjector AddApiProxy<TService>(
            this EasyInjector injector,
            Action<IServiceProvider, ApiProxyBuilder> configApiAction
          )
            where TService : class
        {
            // 註冊 singleton factory
            injector.AddSingleton<IApiProxyFactory<TService>>(sp =>
            {
                var builder = new ApiProxyBuilder();
                configApiAction(sp, builder);
                return builder.Build<TService>();
            });

            // 註冊 scoped api proxy
            injector.AddScoped<IApiProxy<TService>>(sp =>
            {
                var factory = sp.GetRequiredService<IApiProxyFactory<TService>>();
                return factory.Create();
            });

            // 註冊 scoped api proxy
            injector.AddScoped<TService>(sp =>
            {
                var proxy = sp.GetRequiredService<IApiProxy<TService>>();
                return proxy.Api;
            });
            return injector;
        }
    }
}