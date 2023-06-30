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
    public class OptionalTest : BaseTest
    {
        [SetUp]
        public void Init()
        {
            //每個測試方法執行前都會執行的動作
        }

        [Test]
        public void Optional001()
        {           
            using (var injector = new EasyInjector())
            {
                injector.AddScoped<IFtpAdminService>(sp => new FtpAdminService());

                // Optional服務只有實際Get()才會生成
                using (var scope = injector.CreateScope())
                {
                    // 由Scope取得預設Provider為Scope
                    var srvf = scope.ServiceProvider.GetRequiredService<IOptional<IFtpAdminService>>();
                    Assert.True(FtpAdminService.NewCounter == 0);

                    var srv1 = srvf.Get();
                    Assert.True(FtpAdminService.NewCounter == 1);
                    Assert.NotNull(srv1);
                }

                // 由injector取得預設Provider為injector
                var srvf2 = injector.GetRequiredService<IOptional<IFtpAdminService>>();
                using (var scope = injector.CreateScope())
                {
                    // Root 沒有Scope無法取得預設
                    Assert.Throws<ApplicationException>(() => srvf2.Get());

                    // 指定Scope就可順利取得
                    var srv2 = srvf2.Get(scope.ServiceProvider);
                    Assert.True(FtpAdminService.NewCounter == 2);
                    Assert.NotNull(srv2);
                }
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
