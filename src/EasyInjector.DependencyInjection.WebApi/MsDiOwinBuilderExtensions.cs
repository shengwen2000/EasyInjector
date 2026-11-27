using System.Web.Http;
using EasyInjectors.WebApis;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Owin;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Microsoft.Extensions.DependencyInjection OWIN Web API 2 擴充方法
/// </summary>
public static class MsDiOwinBuilderExtensions
{
    /// <summary>
    /// 掛載 Microsoft.Extensions.DependencyInjection 至 Web API 2 HttpConfiguration
    /// </summary>
    public static IAppBuilder UseDependencyInjection(
        this IAppBuilder builder,
        HttpConfiguration configuration,
        IServiceProvider serviceProvider,
        bool ownsProvider = true)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

        configuration.DependencyResolver = new MsDiDependencyResolver(serviceProvider, ownsProvider);
        return builder;
    }
}
