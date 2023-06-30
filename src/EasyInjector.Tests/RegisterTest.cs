using EasyInjectors;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [Category("EasyInjector")]
    [TestFixture]
    public class RegisterTest : BaseTest
    {
        [SetUp]
        public void Init()
        {
            //每個測試方法執行前都會執行的動作
        }

        [Test]
        public void Register001()
        {
            var injector = new EasyInjector();
            {
                injector.AddScoped<IFtpClient>(sp => new FtpClient());

                // 後面註冊會蓋掉前面的
                injector.AddSingleton<IFtpAdminService>(sp => new FtpAdminService(sp.GetRequiredService<IFtpClient>()));
                injector.AddTransient<IFtpAdminService>(sp => new FtpAdminService(sp.GetRequiredService<IFtpClient>()));
                injector.AddScoped<IFtpAdminService>(sp => new FtpAdminService(sp.GetRequiredService<IFtpClient>()));

                Assert.True(injector.OfType<ServiceRegister>().Count(x => x.ServiceTypes.Any(y => y == typeof(IFtpAdminService))) == 1);
                               
                // 沒有Scope會失敗
                Assert.Throws<ApplicationException>(() => injector.GetRequiredService<IFtpAdminService>());               
            }

            //可以匯入其他Inject所有服務
            var injector2 = new EasyInjector();
            injector2.ImportServices(injector);

            using(var scope = injector2.CreateScope()) {
                scope.ServiceProvider.GetRequiredService<IFtpAdminService>();
            }            
        }

        [Test]
        public void Register002()
        {
            using (var injector = new EasyInjector())
            {
                injector.AddScoped<IFtpClient>(sp => new FtpClient());

                //兩介面指向同一服務
                injector.AddService(
                    SimpleLifetimes.Scoped, 
                    new[] { typeof(IFtpAdminService), typeof(FtpAdminService) },
                    sp => new FtpAdminService(sp.GetRequiredService<IFtpClient>()));

                // 會得到同一個
                using (var scope = injector.CreateScope())
                {
                    var srv1 = scope.ServiceProvider.GetRequiredService<IFtpAdminService>();
                    var srv2 = scope.ServiceProvider.GetRequiredService<FtpAdminService>();
                    Assert.AreEqual(srv1, srv2);
                }
            }
        }

        public interface IFtpClient
        {
            void Connect();
            void DownloadFile();
        }

        public class FtpClient : IFtpClient
        {

            public void Connect()
            {
                Console.WriteLine("Connect OK");
            }

            public void DownloadFile()
            {
                Console.WriteLine("Download File OK");
            }
        }


        public interface IFtpAdminService
        {
        }

        public class FtpAdminService : IFtpAdminService, IDisposable
        {
            public static int DisposeCounter = 0;
            private IFtpClient _client;

            public FtpAdminService(IFtpClient client)
            {
                _client = client;
            }

            public void Dispose()
            {
                DisposeCounter++;
            }
        }
    }
}
