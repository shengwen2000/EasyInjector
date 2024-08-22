using EasyApiProxys;
using EasyApiProxys.DemoApis;
using HawkNet;

namespace Tests;

public class ApiProxyTest : BaseTest
{
    [Test, Apartment(ApartmentState.STA)]
    public async Task ApiProxy001()
    {
        // 類視窗環境模擬
        Assert.That(SynchronizationContext.Current, Is.Not.Null);

        var factory = new ApiProxyBuilder()
              .UseDemoApiServerMock()
              .UseDefaultApiProtocol("http://localhost:8081/api/Demo")
              .Build<IDemoApi>();

        var apiproxy = factory.Create();
        //var api = apiproxy.Api;

        var srvInfo = apiproxy.Object.GetServerInfo();
        Assert.That(srvInfo, Is.EqualTo("Demo Server"));

        var ret = await apiproxy.Object.Login(new Login { Account = "david", Password = "123" });
        Assert.That(ret.Account, Is.EqualTo("david"));

        var email = await apiproxy.Object.GetEmail(new TokenInfo { Token = ret.Token });

        Assert.That(email, Is.EqualTo("david@gmail.com"));

        await apiproxy.Object.Logout(new TokenInfo { Token = ret.Token });

        var ex = Assert.Catch<ApiCodeException>(
            () => apiproxy.Object.GetEmail(new TokenInfo { Token = "0" }).GetAwaiter().GetResult());
        Assert.That(ex.Code, Is.EqualTo("EX"));
    }

    /// <summary>
    /// Hawk 驗證
    /// </summary>
    /// <returns></returns>
    [Test, Apartment(ApartmentState.STA)]
    public async Task ApiProxy002()
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
            Assert.Catch<HttpRequestException>(() => proxy.Object.GetServerInfo());
        }

        {
            var factory = new ApiProxyBuilder()
                // Server 啟用Hawk驗證
                .UseDemoApiServerMock(credential)
                .UseDefaultApiProtocol("http://localhost:8081/api/Demo")
                .UseHawkAuthorize(credential)
                .Build<IDemoApi>();

            var proxy = factory.Create();

            var srvInfo = proxy.Object.GetServerInfo();
            Assert.That(srvInfo, Is.EqualTo("Demo Server"));
        }

        {
            var factory = new ApiProxyBuilder()
                // Server 啟用Hawk驗證
                .UseDemoApiServerMock(credential)
                .UseDefaultApiProtocol("http://localhost:8081/api/Demo")
                .Build<IDemoApi>();
            var proxy = factory.Create();

            var ex = Assert.Catch<ApiCodeException>(() => proxy.Object.GetServerInfo());
            Assert.That(ex?.Code, Is.EqualTo("HAWK_FAIL"));
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

        var msg1 = await proxy.Object.RunProc(new ProcInfo { ProcSeconds = 2 });
        Assert.That(msg1, Is.EqualTo("OK 2"));

        Assert.Catch<Exception>(() => proxy.Object.RunProc(new ProcInfo { ProcSeconds = 10 }).GetAwaiter().GetResult());
    }
}