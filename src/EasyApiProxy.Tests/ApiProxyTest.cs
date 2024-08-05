using KmuApps.ApiProxys;
using KmuApps.ApiProxys.Demos;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Tests
{
	[Category("EasyApiProxy")]
	[TestFixture]
    public class ApiProxyTest : BaseTest
	{
		[Test]
		public async Task ApiProxy001()
		{
            var api = new ApiProxyBuilder()
                .UseDemoApiServerMock()
                .UseDefaultApiProtocol("http://localhost:8081/api/Demo")                
                .Build<IDemoApi>();                                

            var srvInfo = api.GetServerInfo();
            Assert.AreEqual("Demo Server", srvInfo);            

            var ret = await api.Login(new KmuApps.ApiProxys.Demos.Login { Account = "david", Password = "123" });
            Assert.True(ret.Account == "david");

            var email = await api.GetEmail(new TokenInfo { Token = ret.Token });

            Assert.AreEqual("david@gmail.com", email);

            await api.Logout(new TokenInfo { Token = ret.Token });

            var ex = Assert.Catch<DefaultApiCodeError>(() => api.GetEmail(new TokenInfo { Token = "0" }).GetAwaiter().GetResult());
            Assert.AreEqual("EX", ex.Code);

		}

        [Test]
        public void ApiProxy002()
        {
            var a = new DefaultApiResult<string>();
            a.Data = "123";

            var b = a as DefaultApiResult;
            Assert.True(b.Data.Equals("123"));
        }
	}
}