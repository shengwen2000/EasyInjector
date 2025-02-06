using EasyApiProxys;
using EasyApiProxys.DemoApis;
using HawkNet;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    [Category("EasyApiProxy_Kmuhome")]
    [TestFixture]
    public class KmuhomeApiTest : BaseTest
    {
        /// <summary>
        /// 一般的API 測試
        /// </summary>
        /// <returns></returns>
        [Test]
        public async void KmuhomeApiTest001()
        {
            // 類視窗環境模擬
            Assert.IsNotNull(SynchronizationContext.Current);

            var factory = new ApiProxyBuilder()
                .UseKmuhomeApiProtocol("http://localhost:5249/api/Demo")
                .Build<IDemoApi>();

            var apiproxy = factory.Create();

            var srvInfo = apiproxy.GetServerInfo();
            Assert.AreEqual("Demo Server", srvInfo);

            var ret = await apiproxy.Login(new Login { Account = "david", Password = "123" });
            Assert.True(ret.Account == "david");
            Assert.AreEqual(ret.Roles.First(), Roles.AdminUser);

            var email = await apiproxy.GetEmail(new TokenInfo { Token = ret.Token });

            Assert.AreEqual("david@gmail.com", email);

            await apiproxy.Logout(new TokenInfo { Token = ret.Token });

            // no result
            apiproxy.NoResult();

            // no result
            await apiproxy.NoResult2();

            // api exception
            var ex = Assert.Catch<ApiCodeException>(() => apiproxy.GetEmail(new TokenInfo { Token = "0" }).GetAwaiter().GetResult());
            Assert.AreEqual("ex", ex.Code);

            Assert.AreEqual(ex.Message, "The Token Not exists");

        }

        /// <summary>
        /// Hawk 驗證失敗
        /// </summary>
        /// <returns></returns>
        [Test]
        public async void KmuhomeApiTest002_NoHawk()
        {
            await Task.FromResult(0);
            {
                var factory = new ApiProxyBuilder()
                    // Server 啟用Hawk驗證                    
                    .UseKmuhomeApiProtocol("http://localhost:5249/api/Demo")
                    .Build<IDemoApi>();

                var proxy = factory.Create();

                // api exception
                var ex = Assert.Catch<HttpRequestException>(() => proxy.HawkApi()
                    .GetAwaiter().GetResult());
                Assert.That(ex.Message, Is.StringContaining("401"));
            }
        }

        /// <summary>
        /// Hawk 驗證
        /// </summary>
        /// <returns></returns>
        [Test]
        public async void KmuhomeApiTest002_Hawk()
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
                    .UseKmuhomeApiProtocol("http://localhost:5249/api/Demo")
                    .UseHawkAuthorize(credential)
                    .Build<IDemoApi>();

                var proxy = factory.Create();

                var ret1 = await proxy.HawkApi();
                Assert.AreEqual(ret1, "hawk api ok");
            }
        }

        /// <summary>
        /// 指定 Mehtod 與 Timeout
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task KmuhomeApiTest003()
        {
            var factory = new ApiProxyBuilder()
                .UseKmuhomeApiProtocol("http://localhost:5249/api/Demo", 20)
                .Build<IDemoApi>();
            var proxy = factory.Create();

            var msg1 = await proxy.RunProc(new ProcInfo { ProcSeconds = 2 });
            Assert.AreEqual("OK 2", msg1);

            Assert.Catch<Exception>(() => proxy.RunProc(new ProcInfo { ProcSeconds = 10 })
                .GetAwaiter().GetResult());
        }

        /// <summary>
        /// Invalidate Model
        /// </summary>
        /// <returns></returns>
        [Test]
        public void KmuhomeApiTest003_Validate1()
        {
            var factory = new ApiProxyBuilder()
                .UseKmuhomeApiProtocol("http://localhost:5249/api/Demo", 20)
                .Build<IDemoApi>();
            var proxy = factory.Create();

            // 觸發 IM Exception
            var ex = Assert.Catch<ApiCodeException>(() => proxy.Login(new Login { Account = "A12345678910", Password = "123" })
                .GetAwaiter().GetResult());
            Assert.That(ex.Code, Is.EqualTo("im"));
            Assert.That(ex.ErrorData, Is.Not.Null);

            var errs = ex.ErrorData as JArray;
            var e1 = errs.First() as JObject;
            var err1 = e1["Account"].Value<string>();
            Assert.That(err1, Is.Not.Null);
        }

        [Test]
        public void KmuhomeApiTest003_Validate2()
        {
            var factory = new ApiProxyBuilder()
                .UseKmuhomeApiProtocol("http://localhost:5249/api/Demo", 20)
                .Build<IDemoApi>();
            var proxy = factory.Create();

            var ex = Assert.Catch<ApiCodeException>(() => proxy.RaiseValidateError()
                .GetAwaiter().GetResult());
            Assert.That(ex.Code, Is.EqualTo("im"));
            Assert.That(ex.ErrorData is JArray, Is.True);
            var errs = ex.ErrorData as JArray;
            var e1 = errs.First() as JObject;
            var err1 = e1["Account"].Value<string>();
            Assert.That(err1, Is.Not.Null);
        } 
    }
}