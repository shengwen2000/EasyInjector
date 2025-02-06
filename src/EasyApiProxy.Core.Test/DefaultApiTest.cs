using System.Text.Json.Nodes;
using EasyApiProxys;
using EasyApiProxys.DemoApis;
using HawkNet;

namespace Tests;

public class DefaultApiTest : BaseTest
{
    [Test, Apartment(ApartmentState.STA)]
    public async Task DefaultApiTest001()
    {
        // 類視窗環境模擬
        Assert.That(SynchronizationContext.Current, Is.Not.Null);

        var factory = new ApiProxyBuilder()
              .UseDefaultApiProtocol("http://localhost:5249/api/Demo")
              .Build<IDemoApi>();

        var apiproxy = factory.Create();
        //var api = apiproxy.Api;

        var srvInfo = apiproxy.GetServerInfo();
        Assert.That(srvInfo, Is.EqualTo("Demo Server"));

        var ret = await apiproxy.Login(new Login { Account = "david", Password = "123" });
        Assert.That(ret.Account, Is.EqualTo("david"));
        Assert.That(ret.Roles.First(), Is.EqualTo(Roles.AdminUser));

        var email = await apiproxy.GetEmail(new TokenInfo { Token = ret.Token });

        Assert.That(email, Is.EqualTo("david@gmail.com"));

        await apiproxy.Logout(new TokenInfo { Token = ret.Token });

        // no result
        apiproxy.NoResult();

        // no result
        await apiproxy.NoResult2();

        // api exception
        var ex = Assert.Catch<ApiCodeException>(
            () => apiproxy.GetEmail(new TokenInfo { Token = "0" }).GetAwaiter().GetResult())
            ?? throw new NullReferenceException();
        Assert.That(ex.Code, Is.EqualTo("EX"));
        Assert.That(ex.Message, Is.EqualTo("The Token Not exists"));
    }

    /// <summary>
    /// Hawk 驗證失敗
    /// </summary>
    /// <returns></returns>
    [Test, Apartment(ApartmentState.STA)]
    public async Task DefaultApiTest002_NoHawk()
    {
        await Task.FromResult(0);
        // 沒有 Hawk驗證
        {
            var factory = new ApiProxyBuilder()
                .UseDefaultApiProtocol("http://localhost:5249/api/Demo", defaltTimeoutSeconds: 60)
                .Build<IDemoApi>();
            var proxy = factory.Create();

            var ex = Assert.CatchAsync<HttpRequestException>(proxy.HawkApi);
            Assert.That(ex.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
        }
    }

    /// <summary>
    /// Hawk 驗證
    /// </summary>
    /// <returns></returns>
    [Test, Apartment(ApartmentState.STA)]
    public async Task DefaultApiTest002_Hawk()
    {
        var credential = new HawkCredential
        {
            Id = "123",
            Key = "werxhqb98rpaxn39848xrunpaw3489ruxnpa98w4rxn",
            Algorithm = "sha256",
            User = "Admin",
        };

        // 啟用Hawk驗證
        {
            var factory = new ApiProxyBuilder()
                .UseDefaultApiProtocol("http://localhost:5249/api/Demo", defaltTimeoutSeconds: 30)
                .UseHawkAuthorize(credential)
                .Build<IDemoApi>();

            var proxy = factory.Create();
            var ret1 = await proxy.HawkApi();
            Assert.That(ret1, Is.EqualTo("hawk api ok"));
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
        var proxy = factory.Create();

        // RunProc 被指定 5秒Timout
        var msg1 = await proxy.RunProc(new ProcInfo { ProcSeconds = 2 });
        // 通過
        Assert.That(msg1, Is.EqualTo("OK 2"));

        // 觸發Timeout
        Assert.Catch<TaskCanceledException>(() => proxy.RunProc(new ProcInfo { ProcSeconds = 10 }).GetAwaiter().GetResult());
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
        var proxy = factory.Create();

        // 觸發 IM Exception
        var ex = Assert.CatchAsync<ApiCodeException>(() =>  proxy.Login(new Login { Account="A12345678910", Password="123"}));
        Assert.That(ex.Code, Is.EqualTo("IM"));
        Assert.That(ex.ErrorData, Is.Not.Null);
        var errs = ex.ErrorData as JsonArray ?? throw new ApplicationException("No error data");
        var e1 = errs.First() as JsonObject;
        var err1 = e1?["Account"]?.GetValue<string>();
        Assert.That(err1, Is.Not.Null);
    }

    [Test]
    public void DefaultApiTest003_Validate2()
    {
        var factory = new ApiProxyBuilder()
            .UseDefaultApiProtocol("http://localhost:5249/api/Demo", 20)
            .Build<IDemoApi>();
        var proxy = factory.Create();

        var ex = Assert.CatchAsync<ApiCodeException>(proxy.RaiseValidateError)
            ?? throw new NullReferenceException();
        Assert.That(ex.Code, Is.EqualTo("IM"));
        Assert.That(ex.ErrorData is JsonArray, Is.True);
         var errs = ex.ErrorData as JsonArray ?? throw new ApplicationException("No error data");
        var e1 = errs.First() as JsonObject;
        var err1 = e1?["Account"]?.GetValue<string>();
        Assert.That(err1, Is.Not.Null);
    }

    [Test]
    public void GetTypeName001()
    {
        {
            var type1 = typeof(IEnumerable<string>);
            var type1name = GetTypeName(type1);
            Assert.That(type1name, Is.EqualTo("IEnumerable<String>"));
        }
        {
            var type1 = typeof(IEnumerable<KeyValuePair<string, DateTime>>);
            var type1name = GetTypeName(type1);
            Assert.That(type1name, Is.EqualTo("IEnumerable<KeyValuePair<String,DateTime>>"));
        }
    }

    string GetTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            using var sw = new StringWriter();
            var pos = type.Name.IndexOf('`');
            sw.Write(type.Name[..pos]);
            sw.Write("<");

            var targs = type.GenericTypeArguments;
            var index = 0;
            foreach (var targ in targs)
            {
                var typeName1 = GetTypeName(targ);
                if (index > 0)
                    sw.Write(",");
                sw.Write(typeName1);
                index++;
            }
            sw.Write(">");
            return sw.ToString();
        }
        else
        {
            return type.Name;
        }
    }
}