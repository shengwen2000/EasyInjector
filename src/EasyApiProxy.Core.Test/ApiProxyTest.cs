using EasyApiProxys;
using EasyApiProxys.DemoApis;

namespace Tests;

public class ApiProxyTest : BaseTest
{
    [Test, Apartment(ApartmentState.STA)]
    public async Task ApiProxy001()
    {
        // 類視窗環境模擬
        Assert.IsNotNull(SynchronizationContext.Current);

        var factory = new ApiProxyBuilder()
              .UseDemoApiServerMock()
              .UseDefaultApiProtocol("http://localhost:8081/api/Demo")
              .Build<IDemoApi>();

        var apiproxy = factory.Create();
        //var api = apiproxy.Api;

        var srvInfo = apiproxy.Object.GetServerInfo();
        Assert.That(srvInfo, Is.EqualTo("Demo Server"));

        var ret = await apiproxy.Object.Login(new Login { Account = "david", Password = "123" });
        Assert.That(ret.Account == "david", Is.True);

        var email = await apiproxy.Object.GetEmail(new TokenInfo { Token = ret.Token });

        Assert.That(email, Is.EqualTo("david@gmail.com"));

        await apiproxy.Object.Logout(new TokenInfo { Token = ret.Token });

        var ex = Assert.Catch<ApiCodeException>(
            () => apiproxy.Object.GetEmail(new TokenInfo { Token = "0" }).GetAwaiter().GetResult());
        Assert.That(ex.Code, Is.EqualTo("EX"));
    }
}