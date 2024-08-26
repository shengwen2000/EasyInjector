using EasyInjectors;
using Microsoft.Extensions.DependencyInjection;

namespace Tests
{
    [Category("EasyInjector")]
    [TestFixture]
    public class ScopeTest : BaseTest
    {
        [Test]
        public void Scope001()
        {
            var services = new ServiceCollection();
            services.AddEasyInjector();
            services.AddScoped<IServiceA, ServiceA>();

            var provider = services.BuildServiceProvider(true);

            // Provider服務只有實際Get()才會生成
            using var scope = provider.CreateScope();
            var srv1 = scope.ServiceProvider.GetRequiredService<IServiceA>();
            Assert.That(srv1.CurrentScope.ServiceProvider, Is.EqualTo(scope.ServiceProvider));
        }

        public interface IServiceA
        {
            IServiceScope CurrentScope {get;}

        }

        public class ServiceA(IServiceScope scope) : IServiceA
        {
            public IServiceScope CurrentScope => scope;
        }
    }
}
