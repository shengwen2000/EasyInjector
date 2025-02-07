using EasyApiProxys;
using EasyApiProxys.DemoApis;
using EasyInjectors;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Tests
{
    /// <summary>
    /// 注入依賴整合
    /// </summary>
	[Category("EasyApiProxy_DI")]
	[TestFixture]
    public class DITest : BaseTest
	{
        [Test]
        public void DITest001()
        {
            var injector = new EasyInjector();

            new ApiProxyBuilder()
                .UseDefaultApiProtocol("http://localhost:5249/api/Demo")
                // 建置整合到注入依賴
                .Build<IDemoApi>(injector);

            var factory1 = injector.GetRequiredService<IApiProxyFactory<IDemoApi>>();
            Assert.That(factory1, Is.Not.Null);

            var factory2 = injector.GetRequiredService<IApiProxyFactory<IDemoApi>>();
            Assert.That(factory1, Is.EqualTo(factory2));

            using (var scope = injector.CreateScope())
            {
                var proxy1 = scope.ServiceProvider.GetRequiredService<IApiProxy<IDemoApi>>();

                Assert.That(proxy1, Is.Not.Null);

                var api1 = scope.ServiceProvider.GetRequiredService<IDemoApi>();

                Assert.That(api1, Is.Not.Null);

                Assert.That(proxy1.Api, Is.EqualTo(api1));
            }
        }       
	}
}