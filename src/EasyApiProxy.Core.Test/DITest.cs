using EasyApiProxys;
using EasyApiProxys.DemoApis;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

[TestFixture]
public class DITest : BaseTest
{
    /// <summary>
    /// 注入依賴整合
    /// </summary>
    [Test]
    public void DITest001()
    {
        var services = new ServiceCollection();

        new ApiProxyBuilder()
            .UseDefaultApiProtocol("http://localhost:5249/api/Demo")
            // 建置整合到注入依賴
            .Build<IDemoApi>(services);

        using var injector = services.BuildServiceProvider();
        var factory1 = injector.GetRequiredService<IApiProxyFactory<IDemoApi>>();
        Assert.That(factory1, Is.Not.Null);

        var factory2 = injector.GetRequiredService<IApiProxyFactory<IDemoApi>>();
        Assert.That(factory1, Is.EqualTo(factory2));

        using var scope = injector.CreateScope();
        var proxy1 = scope.ServiceProvider.GetRequiredService<IApiProxy<IDemoApi>>();

        Assert.That(proxy1, Is.Not.Null);

        var api1 = scope.ServiceProvider.GetRequiredService<IDemoApi>();

        Assert.That(api1, Is.Not.Null);
        Assert.That(proxy1.Api, Is.EqualTo(api1));
    }
}