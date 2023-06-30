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
    public class TransientTest : BaseTest
    {
        [SetUp]
        public void Init()
        {
            //每個測試方法執行前都會執行的動作
        }

        [Test]
        public void Transient001()
        {           
            using (var injector = new EasyInjector())
            {
                injector.AddTransient<IFtpAdminService>(sp => new FtpAdminService());

                // 每次取得 都是新的

                var srv1 = injector.GetRequiredService<IFtpAdminService>();
                Assert.True(FtpAdminService.NewCounter == 1);
                
                using (var scope = injector.CreateScope())
                {
                    var srv2 = scope.ServiceProvider.GetRequiredService<IFtpAdminService>();
                    Assert.True(FtpAdminService.NewCounter == 2);

                    var srv3 = scope.ServiceProvider.GetRequiredService<IFtpAdminService>();
                    Assert.True(FtpAdminService.NewCounter == 3);
                }

                var srv4 = injector.GetRequiredService<IFtpAdminService>();
                Assert.True(FtpAdminService.NewCounter == 4);
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
