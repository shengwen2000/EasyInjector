using EasyApiProxys;

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
    /// - 可以取得 Singleton 服務 IApiProxyFactory<TService>
    /// - 可以取得 Scoped 服務 IApiProxy<TService>
    /// - 可以取得 Scoped 服務 TService
    /// </summary>
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
        services.AddScoped<IApiProxy<TService>>((sp) =>
        {
            var factory = sp.GetRequiredService<IApiProxyFactory<TService>>();
            return factory.Create();
        });

        // 註冊 scoped api proxy
        services.AddScoped<TService>((sp) =>
        {
            var proxy = sp.GetRequiredService<IApiProxy<TService>>();
            return proxy.Api;
        });
        return services;
    }
}