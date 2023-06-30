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
    public class SingletonTest : BaseTest
    {
        [SetUp]
        public void Init()
        {
            //每個測試方法執行前都會執行的動作
        }

        [Test]
        public void Singleton001()
        {
            var count = FtpAdminService.DisposeCounter;
            using (var injector = new EasyInjector())
            {
                injector.AddSingleton<IFtpAdminService>(sp => new FtpAdminService());

                // 可在任何範圍內取得 都是同一個
                var srv1 = injector.GetRequiredService<IFtpAdminService>();
                
                using (var scope = injector.CreateScope())
                {
                    var srv2 = scope.ServiceProvider.GetRequiredService<IFtpAdminService>();
                    Assert.AreEqual(srv1, srv2);
                }
            }

            // Injector結束呼叫Dispose
            Assert.True((count + 1) == FtpAdminService.DisposeCounter);
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
