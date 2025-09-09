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

            // 建置整合到注入依賴
            injector.AddApiProxy<IDemoApi>((sp, builder) => 
                builder.UseDefaultApiProtocol("http://localhost:5249/api/Demo"));

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

        [Test]
        public void DITest002()
        {
            var injector = new EasyInjector();

            // 建置整合到注入依賴
            injector.AddApiProxyNamed<IDemoApi>((sp, builder, name) =>
            {
                if (name == "A")
                {
                    builder.UseDefaultApiProtocol("http://localhost:5249/api/Demo/A");
                }
                else if (name == "B") {
                    builder.UseDefaultApiProtocol("http://localhost:5249/api/Demo/B");
                }
                else
                    throw new System.NotSupportedException("Not Support Named Instance =" + name);
            });            

            var factory1 = injector.GetRequiredService<INamed<IApiProxyFactory<IDemoApi>>>().GetByName("A");
            Assert.That(factory1 != null);
            Assert.That(factory1.Options.BaseUrl == "http://localhost:5249/api/Demo/A");
            
            using (var scope = injector.CreateScope())
            {
                var srv1 = scope.ServiceProvider.GetRequiredService<INamed<IApiProxy<IDemoApi>>>()
                    .GetByName("A");
                Assert.That(srv1 != null);
                Assert.That(srv1.Factory == factory1);                

                var srv2 = scope.ServiceProvider.GetRequiredService<INamed<IDemoApi>>()
                    .GetByName("A");
                Assert.That(srv2 != null);
            }

            var factory2 = injector.GetRequiredService<INamed<IApiProxyFactory<IDemoApi>>>().GetByName("B");
            Assert.That(factory2 != null);
            Assert.That(factory2.Options.BaseUrl == "http://localhost:5249/api/Demo/B");

            using (var scope = injector.CreateScope())
            {
                var srv1 = scope.ServiceProvider.GetRequiredService<INamed<IApiProxy<IDemoApi>>>()
                   .GetByName("B");

                Assert.That(srv1 != null);
                Assert.That(srv1.Factory == factory2);            
                var srv2 = scope.ServiceProvider.GetRequiredService<INamed<IDemoApi>>()
                    .GetByName("B");
                Assert.That(srv2 != null);
            }

            Assert.That(factory1 != factory2);            
        }  
	}
}