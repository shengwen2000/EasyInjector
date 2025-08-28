using EasyApiProxys;
using EasyApiProxys.DemoApis;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

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
            .UseLocalApi<IDemoApi>(sp => new DemoApiLocal())
            .Build<IDemoApi>();

        var proxy1 = factory.Create(null!);
        var api1 = proxy1.Api;

        var srvInfo = api1.GetServerInfo();
        Assert.That(srvInfo, Is.EqualTo("Demo Server"));
    }

    [Test]
    public void LocalApiTest002()
    {
        var services = new ServiceCollection();
        services.AddApiProxy<IDemoApi>((sp, builder) =>
        {
            builder.UseLocalApi<IDemoApi>(sp1 => new DemoApiLocal());
        });

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IApiProxyFactory<IDemoApi>>();
        Assert.That(factory, Is.Not.EqualTo(null));

        var api1 = scope.ServiceProvider.GetRequiredService<IApiProxy<IDemoApi>>();
        Assert.That(api1, Is.Not.EqualTo(null));

        var api2 = scope.ServiceProvider.GetRequiredService<IDemoApi>();
        Assert.That(api2, Is.Not.EqualTo(null));

        Assert.That(api1.Api == api2);

        var srvInfo = api2.GetServerInfo();
        Assert.That(srvInfo, Is.EqualTo("Demo Server"));
    }

    public class DemoApiLocal : IDemoApi
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
    }

}