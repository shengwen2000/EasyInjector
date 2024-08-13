using EasyInjectors;
using EasyInjectors.Dev;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests.Overrides;

namespace Tests
{
    [Category("Common")]
    [TestFixture]
    public class OverrideTest : BaseTest
    {
        [SetUp]
        public void Init()
        {
            //每個測試方法執行前都會執行的動作
        }

        [Test]
        public async void Override001()
        {
            var injector = new EasyInjector();

            injector.AddScoped<ILoginService, LoginService>();

            // 複寫測試類別
            injector.AddOverride<ILoginService, LoginServiceDev>();

            using (var scope = injector.CreateScope())
            {
                var api = scope.ServiceProvider.GetRequiredService<ILoginService>();
                var ret = api.Login(new Login { Account = "david", Password = "{error password}" });
                Assert.True(ret == "OK");

                var ret2 = await api.Login2(new Login { Account = "david", Password = "123456" });
                Assert.True(ret2 == "OK");
            }
        }        
    }

    namespace Overrides
    {
        public interface ILoginService
        {
            string Login(Login req);

            Task<string> Login2(Login req);
        }

        public class LoginService : ILoginService
        {
            public LoginService()
            {
            }

            [Override]
            public string Login(Login req)
            {
                if (req.Account == "david" && req.Password == "123")
                    return "OK";
                return "Account or Password Error";
            }

            public async Task<string> Login2(Login req)
            {
                await Task.Delay(1000);

                if (req.Account == "david" && req.Password == "123456")
                    return "OK";
                return "Account or Password Error";
            }
        }

        public class LoginServiceDev : ILoginService
        {
            private ILoginService _basesrv;

            public LoginServiceDev(ILoginService basesrv)
            {
                _basesrv = basesrv;
            }

            [Override]
            public string Login(Login req)
            {
                req.Account = "david";
                req.Password = "123";
                return _basesrv.Login(req);
            }

            public Task<string> Login2(Login req)
            {
                throw new NotImplementedException();
            }
        }

        public class Login
        {
            public string Account { get; set; }
            public string Password { get; set; }
        }
    }
}
