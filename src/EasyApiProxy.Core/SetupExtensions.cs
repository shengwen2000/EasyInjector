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
    /// 加入 ApiProxy 使用 DefaultApi 協定
    /// - 可以取得 Singleton 服務 IApiProxyFactory<TService>
    /// - 可以取得 Scoped 服務 IApiProxy<TService>
    /// - 可以取得 Scoped 服務 TService
    /// </summary>
    static public IServiceCollection AddDefaultApiProxy<TService>(
        this IServiceCollection services,
        string apiBaseUrl,
        int timeoutSeconds = 30,
        Action<ApiProxyBuilder>? configApiAction = null)
        where TService : class
    {
        var builder = new ApiProxyBuilder()
            .UseDefaultApiProtocol(apiBaseUrl, timeoutSeconds);

        configApiAction?.Invoke(builder);

        builder.Build<TService>(services);
        return services;
    }

    /// <summary>
    /// 加入 ApiProxy 使用 KmuhomeApi 協定
    /// - 可以取得 Singleton 服務 IApiProxyFactory<TService>
    /// - 可以取得 Scoped 服務 IApiProxy<TService>
    /// - 可以取得 Scoped 服務 TService
    /// </summary>
    static public IServiceCollection AddKmuhomeApiProxy<TService>(
        this IServiceCollection services,
        string apiBaseUrl,
        int timeoutSeconds = 30,
        Action<ApiProxyBuilder>? configApiAction = null)
        where TService : class
    {
        var builder = new ApiProxyBuilder()
            .UseKmuhomeApiProtocol(apiBaseUrl, timeoutSeconds);

        configApiAction?.Invoke(builder);

        builder.Build<TService>(services);
        return services;
    }

}