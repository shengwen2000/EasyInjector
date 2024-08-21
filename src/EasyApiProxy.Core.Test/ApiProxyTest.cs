using EasyApiProxys;
using EasyApiProxys.DemoApis;

namespace Tests;

[TestClass]
public class ApiProxyTest : BaseTest
{
    [TestMethod]
    public async Task ApiProxy001()
    {
        // 類視窗環境模擬
        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        Assert.IsNotNull(SynchronizationContext.Current);

        var factory = new ApiProxyBuilder()
              .UseDemoApiServerMock()
              .UseDefaultApiProtocol("http://localhost:8081/api/Demo")
              .Build<IDemoApi>();

        var apiproxy = factory.Create();
        //var api = apiproxy.Api;

        var srvInfo = apiproxy.Object.GetServerInfo();
        Assert.AreEqual("Demo Server", srvInfo);

        var ret = await apiproxy.Object.Login(new Login { Account = "david", Password = "123" });
        Assert.IsTrue(ret.Account == "david");

        var email = await apiproxy.Object.GetEmail(new TokenInfo { Token = ret.Token });

        Assert.AreEqual("david@gmail.com", email);

        await apiproxy.Object.Logout(new TokenInfo { Token = ret.Token });

        var ex = Assert.ThrowsExceptionAsync<ApiCodeException>(
            () => apiproxy.Object.GetEmail(new TokenInfo { Token = "0" }))
            .GetAwaiter().GetResult();
        Assert.AreEqual("EX", ex.Code);
    }
}