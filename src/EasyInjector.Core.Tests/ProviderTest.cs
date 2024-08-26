using EasyInjectors;
using Microsoft.Extensions.DependencyInjection;

namespace Tests
{
    [Category("EasyInjector")]
    [TestFixture]
    public class ProviderTest : BaseTest
    {
        [Test]
        public void Provider001()
        {
            var services = new ServiceCollection();
            services.AddEasyInjector();
            services.AddScoped<IFtpAdminService>(sp => new FtpAdminService());

            var provider = services.BuildServiceProvider(true);

            // Provider服務只有實際Get()才會生成
            using (var scope = provider.CreateScope())
            {
                // 由Scope取得預設Provider為Scope
                var srvf = scope.ServiceProvider.GetRequiredService<IProvider<IFtpAdminService>>();
                Assert.That(FtpAdminService.NewCounter, Is.EqualTo(0));

                var srv1 = srvf.Get();
                Assert.That(FtpAdminService.NewCounter, Is.EqualTo(1));
                Assert.That(srv1, Is.Not.Null);
            }

            // 由injector取得預設Provider為injector
            var srvf2 = provider.GetRequiredService<IProvider<IFtpAdminService>>();
            using (var scope = provider.CreateScope())
            {
                // Root 沒有Scope無法取得預設
                Assert.Throws<InvalidOperationException>(() => srvf2.Get());

                // 指定Scope就可順利取得
                var srv2 = srvf2.Get(scope);
                Assert.That(FtpAdminService.NewCounter, Is.EqualTo(2));
                Assert.That(srv2, Is.Not.Null);
            }

        }

        public interface IFtpAdminService
        {
        }

        public class FtpAdminService : IFtpAdminService
        {
            public static int NewCounter = 0;

            public FtpAdminService()
            {
                NewCounter++;
            }
        }
    }
}
