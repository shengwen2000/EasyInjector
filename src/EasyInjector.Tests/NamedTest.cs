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
            using (var injector = new EasyInjector())
            {
                injector.AddNamedScoped<IFtpAdminService>((sp,name) =>                    
                        new FtpAdminService(name));

                injector.AddScoped<IFtpAdminService>(sp => 
                    sp.GetRequiredService<INamed<IFtpAdminService>>().GetByName("Default"));

                var scopef = injector.GetRequiredService<IServiceScopeFactory>();
                using (var scope = scopef.CreateScope())
                {
                    var srv1 = scope.ServiceProvider.GetRequiredService<IFtpAdminService>();

                    var srvf = scope.ServiceProvider.GetRequiredService<INamed<IFtpAdminService>>();
                    var srv2 = srvf.GetByName("Default");
                    var srv3 = srvf.GetByName("Default");
                    var srv4 = srvf.GetByName("Spring");

                    Assert.True(srv1 == srv2);
                    Assert.True(srv2 == srv3);
                    Assert.True(srv3 != srv4);
                }
            }           
        }

        public interface IFtpAdminService
        {
            string SiteName { get; }
        }

        public class FtpAdminService : IFtpAdminService, IDisposable
        {
            public static int DisposeCounter = 0;

            public FtpAdminService(string siteName)
            {
                SiteName = siteName;
            }

            public void Dispose()
            {
                DisposeCounter++;
            }

            public string SiteName
            {
                get;
                private set;
            }
        }
    }
}
