using System.Web.Http;
using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace EasyInjectors.WebApis;

/// <summary>
/// Microsoft.Extensions.DependencyInjection for ASP.NET Web API 2 IDependencyResolver
/// </summary>
public sealed class MsDiDependencyResolver : IDependencyResolver
{
    private readonly IServiceProvider _rootProvider;
    private readonly bool _ownsProvider;
    private bool _disposed;

    /// <summary>
    /// 建立 Web API 2 DependencyResolver
    /// </summary>
    /// <param name="rootProvider">根 ServiceProvider</param>
    /// <param name="ownsProvider">Dispose Resolver 時是否一併釋放 rootProvider (預設 true)</param>
    public MsDiDependencyResolver(IServiceProvider rootProvider, bool ownsProvider = true)
    {
        _rootProvider = rootProvider ?? throw new ArgumentNullException(nameof(rootProvider));
        _ownsProvider = ownsProvider;
    }

    /// <inheritdoc />
    public IDependencyScope BeginScope()
    {
        var scope = _rootProvider.CreateScope();
        return new MsDiDependencyScope(scope);
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType)
    {
        return _rootProvider.GetService(serviceType);
    }

    /// <inheritdoc />
    public IEnumerable<object> GetServices(Type serviceType)
    {
        return _rootProvider.GetServices(serviceType).Where(x => x != null)!;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_ownsProvider && _rootProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private sealed class MsDiDependencyScope(IServiceScope scope) : IDependencyScope
    {
        public object? GetService(Type serviceType)
        {
            // 若 Controller 未顯式註冊於 DI，回退使用 ActivatorUtilities 自動注入建構子依賴
            if (typeof(ApiController).IsAssignableFrom(serviceType))
            {
                return scope.ServiceProvider.GetService(serviceType)
                    ?? ActivatorUtilities.CreateInstance(scope.ServiceProvider, serviceType);
            }

            return scope.ServiceProvider.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return scope.ServiceProvider.GetServices(serviceType).Where(x => x != null)!;
        }

        public void Dispose()
        {
            scope.Dispose();
        }
    }
}
