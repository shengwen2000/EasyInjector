using EasyApiProxys;
using EasyApiProxys.DemoApis;
using EasyInjectors;
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

        services.AddApiProxy<IDemoApi>((sp, builder) =>
        {
            // 設定 ApiProxy 的選項
            builder.UseDefaultApiProtocol("http://localhost:5249/api/Demo");
            // 可以設定其他選項
            // builder.Options.Timeout = TimeSpan.FromSeconds(10);
        });

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

    [Test]
    public void DITest002()
    {
        var services = new ServiceCollection();

        // 建置整合到注入依賴
        services.AddApiProxyNamed<IDemoApi>((sp, builder, name) =>
        {
            if (name == "A")
            {
                builder.UseDefaultApiProtocol("http://localhost:5249/api/Demo/A");
            }
            else if (name == "B")
            {
                builder.UseDefaultApiProtocol("http://localhost:5249/api/Demo/B");
            }
            else
                throw new System.NotSupportedException("Not Support Named Instance =" + name);
        });

        using var injector = services.BuildServiceProvider();

        var factory1 = injector.GetRequiredService<INamed<IApiProxyFactory<IDemoApi>>>().GetByName("A");
        Assert.That(factory1, Is.Not.EqualTo(null));
        Assert.That(factory1.Options.BaseUrl, Is.EqualTo("http://localhost:5249/api/Demo/A"));

        using (var scope = injector.CreateScope())
        {
            var srv1 = scope.ServiceProvider.GetRequiredService<INamed<IApiProxy<IDemoApi>>>()
                .GetByName("A");
            Assert.That(srv1, Is.Not.EqualTo(null));
            Assert.That(srv1.Factory, Is.EqualTo(factory1));

            var srv2 = scope.ServiceProvider.GetRequiredService<INamed<IDemoApi>>()
                .GetByName("A");
            Assert.That(srv2, Is.Not.EqualTo(null));
        }

        var factory2 = injector.GetRequiredService<INamed<IApiProxyFactory<IDemoApi>>>().GetByName("B");
        Assert.That(factory2, Is.Not.EqualTo(null));
        Assert.That(factory2.Options.BaseUrl, Is.EqualTo("http://localhost:5249/api/Demo/B"));

        using (var scope = injector.CreateScope())
        {
            var srv1 = scope.ServiceProvider.GetRequiredService<INamed<IApiProxy<IDemoApi>>>()
               .GetByName("B");

            Assert.That(srv1, Is.Not.EqualTo(null));
            Assert.That(srv1.Factory, Is.EqualTo(factory2));
            var srv2 = scope.ServiceProvider.GetRequiredService<INamed<IDemoApi>>()
                .GetByName("B");
            Assert.That(srv2, Is.Not.EqualTo(null));
        }

        Assert.That(factory1, Is.Not.EqualTo(factory2));
    }
}