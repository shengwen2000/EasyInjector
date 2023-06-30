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
    public class ScopeTest : BaseTest
    {
        [SetUp]
        public void Init()
        {
            //每個測試方法執行前都會執行的動作
        }

        [Test]
        public void Scope001()
        {
            using (var injector = new EasyInjector())
            {
                injector.AddScoped<IFtpAdminService>(sp => new FtpAdminService());

                // 使用Scope服務必須先建立Scope 
                Assert.Throws<ApplicationException>(() => injector.GetService<IFtpAdminService>());


                // 同一個 Scope取得的必是同一個服務
                using (var scope = injector.CreateScope())
                {

                    var srv1 = scope.ServiceProvider.GetRequiredService<IFtpAdminService>();
                    Assert.NotNull(srv1);
                    var srv2 = scope.ServiceProvider.GetRequiredService<IFtpAdminService>();
                    Assert.AreEqual(srv1, srv2);

                    using (var scope2 = injector.CreateScope())
                    {
                        var srv3 = scope2.ServiceProvider.GetRequiredService<IFtpAdminService>();
                        Assert.AreNotEqual(srv2, srv3);
                        var srv4 = scope2.ServiceProvider.GetRequiredService<IFtpAdminService>();
                        Assert.AreEqual(srv3, srv4);
                    }
                }

                // 離開Scope會呼叫Dispose
                {
                    var count = FtpAdminService.DisposeCounter;
                    using (var scope = injector.CreateScope())
                    {
                        var srv1 = scope.ServiceProvider.GetRequiredService<IFtpAdminService>();
                    }
                    Assert.True((count + 1) == FtpAdminService.DisposeCounter);
                }
            }           
        }

        public interface IFtpAdminService
        {
        }

        public class FtpAdminService : IFtpAdminService, IDisposable
        {
            public static int DisposeCounter = 0;

            public FtpAdminService()
            {
            }

            public void Dispose()
            {
                DisposeCounter++;
            }
        }
    }
}
