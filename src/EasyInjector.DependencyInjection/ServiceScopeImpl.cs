using Microsoft.Extensions.DependencyInjection;

namespace EasyInjectors;

/// <summary>
/// (Scope) 取得目前所處在的 ServiceScope
/// </summary>
public class ServiceScopeImpl(IServiceProvider serviceProvider) : IServiceScope
{
    private readonly IServiceScope _scope = serviceProvider as IServiceScope ?? throw new ApplicationException("Unable to get IServiceScope");

    /// <summary>
    /// 服務提供者
    /// </summary>
    public IServiceProvider ServiceProvider => _scope.ServiceProvider;

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
    {
        //_scope.Dispose();
    }
}