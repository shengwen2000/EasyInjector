using EasyApiProxys;
using EasyInjectors;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// 注入 ApiProxy
/// </summary>
public static class SetupExtensions
{
    /// <summary>
    /// 加入 ApiProxy
    /// - 可以取得 Singleton 服務 IApiProxyFactory&lt;TService&gt;
    /// - 可以取得 Scoped 服務 IApiProxy&lt;TService&gt;
    /// - 可以取得 Scoped 服務 TService
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configApiAction">Action to configure the API proxy</param>
    /// <typeparam name="TService">The service interface type</typeparam>
    /// <returns>The service collection for chaining</returns>
    static public IServiceCollection AddApiProxy<TService>(
        this IServiceCollection services,
        Action<IServiceProvider, ApiProxyBuilder> configApiAction
      )
        where TService : class
    {
        // 註冊 singleton factory
        services.AddSingleton<IApiProxyFactory<TService>>(sp =>
        {
            var builder = new ApiProxyBuilder();
            configApiAction(sp, builder);
            return builder.Build<TService>();
        });

        // 註冊 scoped api proxy
        services.AddScoped<IApiProxy<TService>>(sp =>
        {
            var factory = sp.GetRequiredService<IApiProxyFactory<TService>>();
            return factory.Create(sp);
        });

        // 註冊 scoped api proxy
        services.AddScoped<TService>((sp) =>
        {
            var proxy = sp.GetRequiredService<IApiProxy<TService>>();
            return proxy.Api;
        });
        return services;
    }

    /// <summary>
    /// 加入 ApiProxy 註冊到 easyInjector 注入依賴服務內
    /// - 多名稱實例
    /// - 可以取得 Singleton 服務 IApiProxyFactory of TService
    /// - 可以取得 Scoped 服務 IApiProxy of TService
    /// - 可以取得 Scoped 服務 TService
    /// </summary>
    /// <typeparam name="TService">服務API類別</typeparam>
    /// <param name="services">EasyInjector Instance</param>
    /// <param name="configApiAction">規劃API參數</param>
    static public IServiceCollection AddApiProxyNamed<TService>(
        this IServiceCollection services,
        Action<IServiceProvider, ApiProxyBuilder, string> configApiAction
      )
        where TService : class
    {

        services.AddNamedSingleton<IApiProxyFactory<TService>>((sp, named) =>
        {
            var builder = new ApiProxyBuilder();
            configApiAction(sp, builder, named);
            return builder.Build<TService>();
        });

        services.AddNamedScoped<IApiProxy<TService>>((sp, named) =>
        {
            var factory = sp.GetRequiredService<INamed<IApiProxyFactory<TService>>>()
                .GetByName(named);
            return factory.Create(sp);
        });

        // 註冊 scoped api proxy
        services.AddNamedScoped<TService>((sp, named) =>
        {
            var proxy = sp.GetRequiredService<INamed<IApiProxy<TService>>>()
                .GetByName(named);
            return proxy.Api;
        });

        return services;
    }
}