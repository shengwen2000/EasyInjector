using EasyInjectors;
using Microsoft.Extensions.DependencyInjection;

namespace Tests
{
    [Category("EasyInjector")]
    [TestFixture]
    public class NamedTest : BaseTest
    {
        [SetUp]
        public void Init()
        {
            //每個測試方法執行前都會執行的動作
        }

        [Test]
        public void Named001()
        {
            var services = new ServiceCollection();
            services.AddEasyInjector();
            services.AddNamedScoped<IFtpAdminService>((sp, name) => new FtpAdminService(name));
            services.AddScoped<IFtpAdminService>(sp =>
                sp.GetRequiredService<INamed<IFtpAdminService>>().GetByName("Default"));

            var provider = services.BuildServiceProvider(true);

            using var scope = provider.CreateScope();
            var srv1 = scope.ServiceProvider.GetRequiredService<IFtpAdminService>();

            Assert.That(srv1.SiteName, Is.EqualTo("Default"));

            var srvf = scope.ServiceProvider.GetRequiredService<INamed<IFtpAdminService>>();
            var srv2 = srvf.GetByName("Default");
            Assert.That(srv2.SiteName, Is.EqualTo("Default"));
            var srv3 = srvf.GetByName("Default");
            Assert.That(srv3.SiteName, Is.EqualTo("Default"));

            var srv4 = srvf.GetByName("Spring");
            Assert.That(srv4.SiteName, Is.EqualTo("Spring"));

            Assert.That(srv1, Is.EqualTo(srv2));
            Assert.That(srv2, Is.EqualTo(srv3));
            Assert.That(srv3, Is.Not.EqualTo(srv4));
        }

        public interface IFtpAdminService
        {
            string SiteName { get; }
        }

        public class FtpAdminService(string siteName) : IFtpAdminService, IDisposable
        {
            public static int DisposeCounter = 0;

            public void Dispose()
            {
                DisposeCounter++;
            }

            public string SiteName
            {
                get;
                private set;
            } = siteName;
        }
    }
}
