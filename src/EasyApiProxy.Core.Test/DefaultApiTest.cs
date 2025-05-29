using System.Text.Json;
using System.Text.Json.Nodes;
using EasyApiProxys;
using EasyApiProxys.BasicAuth;
using EasyApiProxys.DemoApis;
using HawkNet;
using Microsoft.Extensions.DependencyInjection;

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

        var proxy1 = factory.Create();
        var api1 = proxy1.Api;

        var srvInfo = api1.GetServerInfo();
        Assert.That(srvInfo, Is.EqualTo("Demo Server"));

        var ret = await api1.Login(new Login { Account = "david", Password = "123" });
        Assert.That(ret.Account, Is.EqualTo("david"));
        Assert.That(ret.Roles.First(), Is.EqualTo(Roles.AdminUser));

        var email = await api1.GetEmail(new TokenInfo { Token = ret.Token });

        Assert.That(email, Is.EqualTo("david@gmail.com"));

        await api1.Logout(new TokenInfo { Token = ret.Token });

        // no result
        api1.NoResult();

        // no result
        await api1.NoResult2();

        // api exception
        var ex = Assert.Catch<ApiCodeException>(
            () => api1.GetEmail(new TokenInfo { Token = "0" }).GetAwaiter().GetResult())
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
            var proxy1 = factory.Create();
            var api1 = proxy1.Api;

            var ex = Assert.CatchAsync<HttpRequestException>(api1.HawkApi);
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

            var proxy1 = factory.Create();
            var api1 = proxy1.Api;
            var ret1 = await api1.HawkApi();
            Assert.That(ret1, Is.EqualTo("hawk api ok"));
        }
    }

    /// <summary>
    /// Basic 驗證失敗
    /// </summary>
    [Test, Apartment(ApartmentState.STA)]
    public async Task DefaultApiTest002_NoBasic()
    {
        await Task.FromResult(0);
        // 沒有 Basic驗證
        {
            var factory = new ApiProxyBuilder()
                .UseDefaultApiProtocol("http://localhost:5249/api/Demo", defaltTimeoutSeconds: 60)
                .Build<IDemoApi>();
            var proxy1 = factory.Create();
            var api1 = proxy1.Api;

            var ex = Assert.CatchAsync<HttpRequestException>(api1.BasicApi);
            Assert.That(ex.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
        }
    }

    /// <summary>
    /// Basic 驗證
    /// </summary>
    /// <returns></returns>
    [Test, Apartment(ApartmentState.STA)]
    public async Task DefaultApiTest002_Basic()
    {
        var credential = new BasicCredential
        {
            Account = "admin",
            PassCode = "admin1234"
        };

        // 啟用Basic驗證
        {
            var factory = new ApiProxyBuilder()
                .UseDefaultApiProtocol("http://localhost:5249/api/Demo", defaltTimeoutSeconds: 30)
                .UseBasicAuthorize(credential)
                .Build<IDemoApi>();

            var proxy1 = factory.Create();
            var api1 = proxy1.Api;
            var ret1 = await api1.BasicApi();
            Assert.That(ret1, Is.EqualTo("basic api ok"));
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

        // RunProc 被指定 5秒Timout
        var msg1 = await api1.RunProc(new ProcInfo { ProcSeconds = 2 });
        // 通過
        Assert.That(msg1, Is.EqualTo("OK 2"));

        // 觸發Timeout
        Assert.Catch<TaskCanceledException>(() => api1.RunProc(new ProcInfo { ProcSeconds = 10 }).GetAwaiter().GetResult());
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
        var ex = Assert.CatchAsync<ApiCodeException>(() => api1.Login(new Login { Account = "A12345678910", Password = "123" }));
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
        var proxy1 = factory.Create();
        var api1 = proxy1.Api;
        var ex = Assert.CatchAsync<ApiCodeException>(api1.RaiseValidateError)
            ?? throw new NullReferenceException();
        Assert.That(ex.Code, Is.EqualTo("IM"));
        Assert.That(ex.ErrorData is JsonArray, Is.True);
        var errs = ex.ErrorData as JsonArray ?? throw new ApplicationException("No error data");
        var e1 = errs.First() as JsonObject;
        var err1 = e1?["Account"]?.GetValue<string>();
        Assert.That(err1, Is.Not.Null);
    }

    [Test]
    public async Task DefaultApiTest004_BearerToken()
    {
        var factory = new ApiProxyBuilder()
            .UseDefaultApiProtocol("http://localhost:5249/api/Demo", 20)
            .Build<IDemoApi>();

        var proxy1 = factory.Create();
        var api1 = proxy1.Api;

        var token1 = "BEARERTOKEN1";
        proxy1.SetBearer(token1);

        bool before1 = false;
        proxy1.BeforeHttpPost = ctx =>
        {
            before1 = true;
            Assert.That(ctx.Request!.Headers.Authorization!.Scheme, Is.EqualTo("Bearer"));
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

    [Test]
    public void DefaultApiTest005_ApiException()
    {
        var factory = new ApiProxyBuilder()
            .UseDefaultApiProtocol("http://localhost:5249/api/Demo", 20)
            .Build<IDemoApi>();
        var proxy1 = factory.Create();
        var api1 = proxy1.Api;
        var data1 = new { a = 123, b = "abc" };
        var req1 = new DefaultApiResult
        {
            Result = "DEMO1",
            Message = "DEMO1MSG",
            Data = data1
        };
        var ex1 = Assert.Catch<ApiCodeException>(() =>
            api1.ThrowApiException(req1));

        Assert.That(ex1.Code, Is.EqualTo(req1.Result));
        Assert.That(ex1.Message, Is.EqualTo(req1.Message));
        Assert.That(ex1.ErrorData, Is.Not.Null);
        var data2 = (JsonElement)ex1!.ErrorData!;
        // 這個因為使用不同的Json庫 所以有差異 .net framework 會得到匿名類別
        Assert.That(data2.GetProperty("a").GetInt32() == data1.a, Is.True);
        Assert.That(data2.GetProperty("b").GetString() == data1.b, Is.True);
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

    [Test]
    public void AddApiTest()
    {
        var services = new ServiceCollection();

        services.AddApiProxy<IDemoApi>(
            configApiAction: (sp, builder) =>
            {
                // 設定 ApiProxy 的預設協定
                builder.UseDefaultApiProtocol("http://localhost:5249/api/Demo",
                    defaltTimeoutSeconds: 30);
                // 設定 Basic 驗證
                builder.UseBasicAuthorize(new BasicCredential
                {
                    Account = "admin",
                    PassCode = "admin1234"
                });
            });

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IApiProxyFactory<IDemoApi>>();
        Assert.That(factory, Is.Not.Null);

        using var scope = provider.CreateScope();
        var proxy1 = scope.ServiceProvider.GetRequiredService<IApiProxy<IDemoApi>>();
        var api1 = scope.ServiceProvider.GetRequiredService<IDemoApi>();
        Assert.That(proxy1, Is.Not.Null);
        Assert.That(api1, Is.Not.Null);
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