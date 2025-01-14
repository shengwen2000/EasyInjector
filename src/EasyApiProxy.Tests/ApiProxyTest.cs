using Castle.DynamicProxy;
using EasyApiProxys;
using EasyApiProxys.DemoApis;
using HawkNet;
using NUnit.Framework;
using System;
using System.Collections;
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

            var factory = new ApiProxyBuilder()
                .UseDemoApiServerMock()
                .UseDefaultApiProtocol("http://localhost:8081/api/Demo")
                .Build<IDemoApi>();

            var apiproxy = factory.Create();

            var srvInfo = apiproxy.GetServerInfo();
            Assert.AreEqual("Demo Server", srvInfo);

            var ret = await apiproxy.Login(new Login { Account = "david", Password = "123" });
            Assert.True(ret.Account == "david");

            var email = await apiproxy.GetEmail(new TokenInfo { Token = ret.Token });

            Assert.AreEqual("david@gmail.com", email);

            await apiproxy.Logout(new TokenInfo { Token = ret.Token });

            var ex = Assert.Catch<ApiCodeException>(() => apiproxy.GetEmail(new TokenInfo { Token = "0" }).GetAwaiter().GetResult());
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
                var factory = new ApiProxyBuilder()
                    // Server 啟用Hawk驗證
                    //.UseDemoApiServerMock(credential)
                    .UseDefaultApiProtocol("http://localhost:8081/api/notfound")
                    .UseHawkAuthorize(credential)
                    .Build<IDemoApi>();

                var proxy = factory.Create();
                // 不存在的網址會觸發異常
                Assert.Catch<HttpRequestException>(() => proxy.GetServerInfo());
            }

            {
                var factory = new ApiProxyBuilder()
                    // Server 啟用Hawk驗證
                    .UseDemoApiServerMock(credential)
                    .UseDefaultApiProtocol("http://localhost:8081/api/Demo")
                    .UseHawkAuthorize(credential)
                    .Build<IDemoApi>();

                var proxy = factory.Create();

                var srvInfo = proxy.GetServerInfo();
                Assert.AreEqual("Demo Server", srvInfo);
            }

            {
                var factory = new ApiProxyBuilder()
                    // Server 啟用Hawk驗證
                    .UseDemoApiServerMock(credential)
                    .UseDefaultApiProtocol("http://localhost:8081/api/Demo")
                    .Build<IDemoApi>();
                var proxy = factory.Create();

                var ex = Assert.Catch<ApiCodeException>(() => proxy.GetServerInfo());
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
            var factory = new ApiProxyBuilder()
                .UseDemoApiServerMock()
                .UseDefaultApiProtocol("http://localhost:8081/api/Demo", 20)
                .Build<IDemoApi>();
            var proxy = factory.Create();

            var msg1 = await proxy.RunProc(new ProcInfo { ProcSeconds = 2 });
            Assert.AreEqual("OK 2", msg1);

            Assert.Catch<Exception>(() => proxy.RunProc(new ProcInfo { ProcSeconds = 10 }).GetAwaiter().GetResult());
        }

        /// <summary>
        /// 指定 Mehtod 與 Timeout
        /// </summary>
        /// <returns></returns>
        //[Test]
        public async Task ApiProxy004()
        {
            var factory = new ApiProxyBuilder()
                //.UseDemoApiServerMock()
                .UseDefaultApiProtocol("http://localhost:8081/api/Demo", 20)
                .Build<IDemoApi>();
            var proxy = factory.Create();

            try
            {
                await proxy.GetEmail(new TokenInfo { Token = "123" });
            }
            catch (Exception ex)
            {
            }
        }


	}
}