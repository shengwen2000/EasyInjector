using EasyApiProxys;
using EasyApiProxys.BasicAuth;
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
    [Category("EasyApiProxy")]
    [TestFixture]
    public class DefaultApiTest : BaseTest
    {
        /// <summary>
        /// 一般的API 測試
        /// </summary>
        /// <returns></returns>
        [Test]
        public async void DefaultApiTest001()
        {
            // 類視窗環境模擬
            Assert.IsNotNull(SynchronizationContext.Current);

            var factory = new ApiProxyBuilder()
                .UseDefaultApiProtocol("http://localhost:5249/api/Demo")
                .Build<IDemoApi>();

            var proxy1 = factory.Create();
            var api1 = proxy1.Api;

            var srvInfo = api1.GetServerInfo();
            Assert.AreEqual("Demo Server", srvInfo);

            var ret = await api1.Login(new Login { Account = "david", Password = "123" });
            Assert.True(ret.Account == "david");
            Assert.AreEqual(ret.Roles.First(), Roles.AdminUser);

            var email = await api1.GetEmail(new TokenInfo { Token = ret.Token });

            Assert.AreEqual("david@gmail.com", email);

            await api1.Logout(new TokenInfo { Token = ret.Token });

            // no result
            api1.NoResult();

            // no result
            await api1.NoResult2();

            // api exception
            var ex = Assert.Catch<ApiCodeException>(() => api1.GetEmail(new TokenInfo { Token = "0" }).GetAwaiter().GetResult());
            Assert.AreEqual("EX", ex.Code);

            Assert.AreEqual(ex.Message, "The Token Not exists");

        }

        /// <summary>
        /// Hawk 驗證失敗
        /// </summary>
        /// <returns></returns>
        [Test]
        public async void DefaultApiTest002_NoHawk()
        {
            await Task.FromResult(0);
            {
                var factory = new ApiProxyBuilder()
                    // Server 啟用Hawk驗證                    
                    .UseDefaultApiProtocol("http://localhost:5249/api/Demo")
                    .Build<IDemoApi>();

                var proxy1 = factory.Create();
                var api1 = proxy1.Api;

                // api exception
                var ex = Assert.Catch<HttpRequestException>(() => api1.HawkApi()
                    .GetAwaiter().GetResult());
                Assert.That(ex.Message, Is.StringContaining("401"));
            }
        }

        /// <summary>
        /// Hawk 驗證
        /// </summary>
        /// <returns></returns>
        [Test]
        public async void DefaultApiTest002_Hawk()
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
                    .UseDefaultApiProtocol("http://localhost:5249/api/Demo")
                    .UseHawkAuthorize(credential)
                    .Build<IDemoApi>();

                var proxy1 = factory.Create();
                var api1 = proxy1.Api;

                var ret1 = await api1.HawkApi();
                Assert.AreEqual(ret1, "hawk api ok");
            }
        }

        /// <summary>
        /// Basic 驗證失敗
        /// </summary>
        /// <returns></returns>
        [Test]
        public async void DefaultApiTest002_NoBasic()
        {
            await Task.FromResult(0);
            {
                var factory = new ApiProxyBuilder()
                    // Server 啟用Hawk驗證                    
                    .UseDefaultApiProtocol("http://localhost:5249/api/Demo")
                    .Build<IDemoApi>();

                var proxy1 = factory.Create();
                var api1 = proxy1.Api;

                // api exception
                var ex = Assert.Catch<HttpRequestException>(() => api1.BasicApi()
                    .GetAwaiter().GetResult());
                Assert.That(ex.Message, Is.StringContaining("401"));
            }
        }

        /// <summary>
        /// Basic 驗證
        /// </summary>
        /// <returns></returns>
        [Test]
        public async void DefaultApiTest002_Basic()
        {
            await Task.FromResult(0);

            var credential = new BasicCredential
            {
                Account = "admin",
                PassCode = "admin1234",
            };

            {
                var factory = new ApiProxyBuilder()
                    // Server 啟用Basic驗證                    
                    .UseDefaultApiProtocol("http://localhost:5249/api/Demo")
                    .UseBasicAuthorize(credential)
                    .Build<IDemoApi>();

                var proxy1 = factory.Create();
                var api1 = proxy1.Api;

                var ret1 = await api1.BasicApi();
                Assert.AreEqual(ret1, "basic api ok");
            }
        }      

        /// <summary>
        /// 指定 Mehtod 與 Timeout
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task DefaultApiTest003()
        {
            var factory = new ApiProxyBuilder()
                .UseDefaultApiProtocol("http://localhost:5249/api/Demo", 20)
                .Build<IDemoApi>();
            var proxy1 = factory.Create();
            var api1 = proxy1.Api;

            var msg1 = await api1.RunProc(new ProcInfo { ProcSeconds = 2 });
            Assert.AreEqual("OK 2", msg1);

            Assert.Catch<Exception>(() => api1.RunProc(new ProcInfo { ProcSeconds = 10 })
                .GetAwaiter().GetResult());
        }



        /// <summary>
        /// Invalidate Model
        /// </summary>
        /// <returns></returns>
        [Test]
        public void DefaultApiTest003_Validate1()
        {
            var factory = new ApiProxyBuilder()
                .UseDefaultApiProtocol("http://localhost:5249/api/Demo", 20)
                .Build<IDemoApi>();
            var proxy1 = factory.Create();
            var api1 = proxy1.Api;

            // 觸發 IM Exception
            var ex = Assert.Catch<ApiCodeException>(() => api1.Login(new Login { Account = "A12345678910", Password = "123" })
                .GetAwaiter().GetResult());
            Assert.That(ex.Code, Is.EqualTo("IM"));
            Assert.That(ex.ErrorData, Is.Not.Null);

            var errs = ex.ErrorData as JArray;
            var e1 = errs.First() as JObject;
            var err1 = e1["Account"].Value<string>();
            Assert.That(err1, Is.Not.Null);
        }

        [Test]
        public void DefaultApiTest003_Validate2()
        {
            var factory = new ApiProxyBuilder()
                .UseDefaultApiProtocol("http://localhost:5249/api/Demo", 20)
                .Build<IDemoApi>();
            var proxy1 = factory.Create();
            var api1 = proxy1.Api;

            var ex = Assert.Catch<ApiCodeException>(() => api1.RaiseValidateError()
                .GetAwaiter().GetResult());
            Assert.That(ex.Code, Is.EqualTo("IM"));
            Assert.That(ex.ErrorData is JArray, Is.True);
            var errs = ex.ErrorData as JArray;
            var e1 = errs.First() as JObject;
            var err1 = e1["Account"].Value<string>();
            Assert.That(err1, Is.Not.Null);
        }

        [Test]
        public async Task DefaultApiTest004_BearerToken()
        {
            var factory = new ApiProxyBuilder()
                .UseDefaultApiProtocol("http://localhost:5249/api/Demo", 20)
                .Build<IDemoApi>();

            using (var proxy1 = factory.Create())
            {
                var api1 = proxy1.Api;

                var token1 = "BEARERTOKEN1";
                proxy1.SetBearer(token1);

                bool before1 = false;
                proxy1.BeforeHttpPost = ctx =>
                {
                    before1 = true;
                    Assert.That(ctx.Request.Headers.Authorization.Scheme, Is.EqualTo("Bearer"));
                    Assert.That(ctx.Request.Headers.Authorization.Parameter, Is.EqualTo(token1));
                };

                bool after1 = false;
                proxy1.AfterHttpPost = ctx =>
                {
                    after1 = true;
                    Assert.That(ctx.Response, Is.Not.Null);
                    Assert.That(ctx.Result, Is.EqualTo(token1));
                };

                var token2 = await api1.GetBearerToken();
                Assert.That(token1, Is.EqualTo(token2));

                Assert.That(before1, Is.True);
                Assert.That(after1, Is.True);
            }            
        }

        [Test]
        public void DefaultApiTest005_ApiException()
        {
            var factory = new ApiProxyBuilder()
                .UseDefaultApiProtocol("http://localhost:5249/api/Demo", 20)
                .Build<IDemoApi>();
            var proxy1 = factory.Create();
            var api1 = proxy1.Api;
            var data1 = new { a = 123, b = "abc" };
            var req1 = new DefaultApiResult { 
                Result = "DEMO1",
                Message = "DEMO1MSG",
                Data = data1
            };
            var ex1 = Assert.Catch<ApiCodeException>(() =>
                api1.ThrowApiException(req1));

            Assert.That(ex1.Code, Is.EqualTo(req1.Result));
            Assert.That(ex1.Message, Is.EqualTo(req1.Message));
            Assert.That(ex1.ErrorData, Is.Not.Null);
            dynamic a = ex1.ErrorData;
            Assert.That(a.a == data1.a, Is.True);
            Assert.That(a.b == data1.b, Is.True);

        }

    }
}