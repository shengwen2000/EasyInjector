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
        /// �@�몺API ����
        /// </summary>
        /// <returns></returns>
		[Test]
		public async void ApiProxy001()
		{   
            // ���������Ҽ���
            Assert.IsNotNull(SynchronizationContext.Current);

            var factory = new ApiProxyBuilder()
                .UseDemoApiServerMock()
                .UseDefaultApiProtocol("http://localhost:8081/api/Demo")                
                .Build<IDemoApi>();

            var apiproxy = factory.Create();
            //var api = apiproxy.Api;

            var srvInfo = apiproxy.Api.GetServerInfo();
            Assert.AreEqual("Demo Server", srvInfo);

            var ret = await apiproxy.Api.Login(new Login { Account = "david", Password = "123" });
            Assert.True(ret.Account == "david");

            var email = await apiproxy.Api.GetEmail(new TokenInfo { Token = ret.Token });

            Assert.AreEqual("david@gmail.com", email);

            await apiproxy.Api.Logout(new TokenInfo { Token = ret.Token });

            var ex = Assert.Catch<ApiCodeException>(() => apiproxy.Api.GetEmail(new TokenInfo { Token = "0" }).GetAwaiter().GetResult());
            Assert.AreEqual("EX", ex.Code);

		}

        /// <summary>
        /// Hawk ����
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
                    // Server �ҥ�Hawk����
                    //.UseDemoApiServerMock(credential)
                    .UseDefaultApiProtocol("http://localhost:8081/api/notfound")
                    .UseHawkAuthorize(credential)
                    .Build<IDemoApi>();

                var proxy = factory.Create();
                // ���s�b�����}�|Ĳ�o���`
                Assert.Catch<HttpRequestException>(() => proxy.Api.GetServerInfo());                
            }

            {
                var factory = new ApiProxyBuilder()
                    // Server �ҥ�Hawk����
                    .UseDemoApiServerMock(credential)
                    .UseDefaultApiProtocol("http://localhost:8081/api/Demo")
                    .UseHawkAuthorize(credential)
                    .Build<IDemoApi>();

                var proxy = factory.Create();

                var srvInfo = proxy.Api.GetServerInfo();
                Assert.AreEqual("Demo Server", srvInfo);
            }

            {
                var factory = new ApiProxyBuilder()
                    // Server �ҥ�Hawk����
                    .UseDemoApiServerMock(credential)
                    .UseDefaultApiProtocol("http://localhost:8081/api/Demo")
                    .Build<IDemoApi>();
                var proxy = factory.Create();

                var ex = Assert.Catch<ApiCodeException>(() => proxy.Api.GetServerInfo());
                Assert.True(ex.Code == "HAWK_FAIL");
            }            
        }

        /// <summary>
        /// ���w Mehtod �P Timeout
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

            var msg1 = await proxy.Api.RunProc(new ProcInfo { ProcSeconds = 2 });
            Assert.AreEqual("OK 2", msg1);

            Assert.Catch<Exception>(() => proxy.Api.RunProc(new ProcInfo { ProcSeconds = 10 }).GetAwaiter().GetResult());
        }


	}
}