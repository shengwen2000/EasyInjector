using EasyApiProxys;
using EasyApiProxys.DemoApis;
using HawkNet;
using NUnit.Framework;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


namespace Tests
{
	[Category("EasyApiProxy")]
	[TestFixture]
    public class ApiProxyTest : BaseTest
	{
        /// <summary>
        /// 一般的API 測試
        /// </summary>
        /// <returns></returns>
		[Test]
		public async void ApiProxy001()
		{   
            // 類視窗環境模擬
            Assert.IsNotNull(SynchronizationContext.Current);

            var api = new ApiProxyBuilder()
                .UseDemoApiServerMock()
                .UseDefaultApiProtocol("http://localhost:8081/api/Demo")                
                .Build<IDemoApi>();                                

            var srvInfo = api.GetServerInfo();
            Assert.AreEqual("Demo Server", srvInfo);            

            var ret = await api.Login(new Login { Account = "david", Password = "123" });
            Assert.True(ret.Account == "david");

            var email = await api.GetEmail(new TokenInfo { Token = ret.Token });

            Assert.AreEqual("david@gmail.com", email);

            await api.Logout(new TokenInfo { Token = ret.Token });

            var ex = Assert.Catch<ApiCodeException>(() => api.GetEmail(new TokenInfo { Token = "0" }).GetAwaiter().GetResult());
            Assert.AreEqual("EX", ex.Code);

		}

        /// <summary>
        /// Hawk 驗證
        /// </summary>
        /// <returns></returns>
        [Test]
        public async void ApiProxy002()
        {
            await Task.FromResult(0);

            var credential = new HawkCredential
            {
                Id = "123",
                Key = "werxhqb98rpaxn39848xrunpaw3489ruxnpa98w4rxn",
                Algorithm = "sha256",
                User = "Admin",
            };
            
            {
                var api = new ApiProxyBuilder()
                    // Server 啟用Hawk驗證
                    //.UseDemoApiServerMock(credential)
                    .UseDefaultApiProtocol("http://localhost:8081/api/notfound")
                    .UseHawkAuthorize(credential)
                    .Build<IDemoApi>();
                // 不存在的網址會觸發異常
                Assert.Catch<HttpRequestException>(() => api.GetServerInfo());                
            }

            {
                var api = new ApiProxyBuilder()
                    // Server 啟用Hawk驗證
                    .UseDemoApiServerMock(credential)
                    .UseDefaultApiProtocol("http://localhost:8081/api/Demo")
                    .UseHawkAuthorize(credential)
                    .Build<IDemoApi>();

                var srvInfo = api.GetServerInfo();
                Assert.AreEqual("Demo Server", srvInfo);
            }

            {
                var api = new ApiProxyBuilder()
                    // Server 啟用Hawk驗證
                    .UseDemoApiServerMock(credential)
                    .UseDefaultApiProtocol("http://localhost:8081/api/Demo")
                    .Build<IDemoApi>();

                var ex = Assert.Catch<ApiCodeException>(() => api.GetServerInfo());
                Assert.True(ex.Code == "HAWK_FAIL");
            }            
        }

        /// <summary>
        /// 指定 Mehtod 與 Timeout
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task ApiProxy003()
        {
            var api = new ApiProxyBuilder()
                .UseDemoApiServerMock()
                .UseDefaultApiProtocol("http://localhost:8081/api/Demo", 20)
                .Build<IDemoApi>();

            var msg1 = await api.RunProc(new ProcInfo { ProcSeconds = 2 });
            Assert.AreEqual("OK 2", msg1);

            Assert.Catch<Exception>(() => api.RunProc(new ProcInfo { ProcSeconds = 10 }).GetAwaiter().GetResult());
        }


	}
}