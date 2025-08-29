using EasyApiProxys;
using EasyApiProxys.BasicAuth;
using EasyApiProxys.DemoApis;
using EasyInjectors;
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
    public class LocalApiTest : BaseTest
    {
        /// <summary>
        /// 一般的API 測試
        /// </summary>
        /// <returns></returns>
        [Test]
        public void LocalApiTest001()
        {
            // 類視窗環境模擬
            //Assert.IsNotNull(SynchronizationContext.Current);

            var factory = new ApiProxyBuilder()
                .UseLocalApi<IDemoApi>(sp => new DempApiLocal())
                .Build<IDemoApi>();

            var proxy1 = factory.Create(null);
            var api1 = proxy1.Api;

            var srvInfo = api1.GetServerInfo();
            Assert.AreEqual("Demo Server", srvInfo);          

        }

        [Test]
        public void LocalApiTest002()
        {
            var injector = new EasyInjectors.EasyInjector();
            injector.AddApiProxy<IDemoApi>((sp, builder) => {
                builder.UseLocalApi<IDemoApi>(sp1 => new DempApiLocal());            
            });

            using (var scope = injector.CreateScope())
            {
                var factory = scope.ServiceProvider.GetRequiredService<IApiProxyFactory<IDemoApi>>();
                Assert.That(factory != null);               

                var api1 = scope.ServiceProvider.GetRequiredService<IApiProxy<IDemoApi>>();
                Assert.That(api1 != null);                

                var api2 = scope.ServiceProvider.GetRequiredService<IDemoApi>();
                Assert.That(api2 != null);

                Assert.That(api1.Api == api2);

                var srvInfo = api2.GetServerInfo();
                Assert.AreEqual("Demo Server", srvInfo);          
            }
        }

        public class DempApiLocal : IDemoApi
        {

            public string GetServerInfo()
            {
                return "Demo Server";
            }

            public Task RaiseValidateError()
            {
                throw new NotImplementedException();
            }

            public void NoResult()
            {
                throw new NotImplementedException();
            }

            public Task NoResult2()
            {
                throw new NotImplementedException();
            }

            public Task<string> HawkApi()
            {
                throw new NotImplementedException();
            }

            public Task<string> BasicApi()
            {
                throw new NotImplementedException();
            }

            public Task<string> GetBearerToken()
            {
                throw new NotImplementedException();
            }

            public void ThrowApiException(DefaultApiResult req)
            {
                throw new NotImplementedException();
            }

            public Task<AccountInfo> Login(Login req)
            {
                throw new NotImplementedException();
            }

            public Task Logout(TokenInfo req)
            {
                throw new NotImplementedException();
            }

            public Task<string> GetEmail(TokenInfo req)
            {
                throw new NotImplementedException();
            }

            public Task<string> RunProc(ProcInfo info)
            {
                throw new NotImplementedException();
            }

            public TokenInfo GetTokenInfo()
            {
                throw new NotImplementedException();
            }
        }        

    }
}