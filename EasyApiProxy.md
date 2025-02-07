# EasyApiProxy

* 簡單的 Web Api 代理類別產生
* 支援Hawk驗證 與 BearerToken驗證
* [Nuget Package Install](https://www.nuget.org/packages/EasyApiProxy/)
* 提供 DefaultApi協定 實作支援 於套件 EasyApiProxy.WebApi
* 提供 用戶端 Hawk驗證 於套件 EasyApiProxy.HawkAuth

## 呼叫範例
``` C#
    // 建立API 代理物件 Factory 必須重複使用 因為其實做包含一個專用的 HttpClient
    // HttpClient 微軟官方明確的說必須公用 否則回有tcp/ip資源不足的問題
    var factory = new ApiProxyBuilder()
        // 套用 DefaultApi 通訊協議
        .UseDefaultApiProtocol("http://localhost:8081/api/Demo")
        .Build<IDemoApi>();

	// 建立代理物件
	using (var proxy = factory.Create()) {

        var api = proxy.Api;

        // 呼叫Api
        var ret1 = await api.Login(new Login { Account = "david", Password = "123" });

        // 傳遞Bearer Token 後續呼叫會自動帶上
        proxy.SetBearer(ret1.Token);

    }


```



## Api 介面定義
``` C#
	// 展示Api 定義
    // 注意 DefaultApi協定 輸入參數只能0或1個。
    public interface IDemoApi
    {
        Task<AccountInfo> Login(Login req);

        Task Logout(TokenInfo req);

        Task<string> GetEmail(TokenInfo req);

        string GetServerInfo();
    }
```

## 後台實作範例 Microsoft.AspNet.WebApi.Owin
- 參考套件 EasyApiProxy.WebApi 提供 DefaultApiResult 實作
``` C#
    /// <summary>
    /// 範例API
    /// </summary>
    [DefaultApiResult] // 套用DefaultApi 通訊封裝
    public partial class DemoApiController : ApiController, IDemoApi
    {
        [HttpPost]
        public async Task<AccountInfo> Login(Login req)
        {
            await Task.Delay(1000);
            if (req.Account == "david" && req.Password == "123")
            {
                return new AccountInfo { Account = req.Account, Token = "123456789", Expired = DateTime.Now.AddHours(1) };
            }
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task Logout(TokenInfo req)
        {
            await Task.Delay(1000);
            if (req.Token == "123456789")
                return;
            throw new ApplicationException("The Token Not exists");
        }

        [HttpPost]
        public async Task<string> GetEmail(TokenInfo req)
        {
            await Task.Delay(1000);
            if (req.Token == "123456789")
            {
                return "david@gmail.com";
            }
            throw new ApplicationException("The Token Not exists");
        }

        [HttpPost]
        public string GetServerInfo()
        {
            return "Demo Server";
        }
    }

    // Startup 啟動規劃WebApi
    public class Startup
    {
        public void Configuration(IAppBuilder app) {

            var apiConfig = new System.Web.Http.HttpConfiguration();

            // 啟用WebApi
            {
                // use attibute routes
                config.MapHttpAttributeRoutes();

                config.Routes.MapHttpRoute(
                   name: "DefaultApi",
                   routeTemplate: "api/{controller}/{action}/{id}",
                   defaults: new { id = RouteParameter.Optional }
                );

                // 必須套用 DefaultApi Json 設定
                config.Formatters.JsonFormatter.SerializerSettings = DefaultApiExtension.DefaultJsonSerializerSettings;

                //use webapi
                app.UseWebApi(apiConfig);
            }
        }
    }
```

## 用戶端啟用HAWK驗證
- 參考套件 EasyApiProxy.HawkAuth
``` C#
    // hawk 證書
    var credential = new HawkNet.HawkCredential();
    credential.Id = "API";
    credential.Key = "XXXXXXXX";
    credential.Algorithm = "sha256";

    // proxy factory
    var factory = new ApiProxyBuilder()
        // 套用 DefaultApi 通訊協議
        .UseDefaultApiProtocol("http://localhost:8081/api/Demo")
        // 啟用Hawk驗證
        .UseHawkAuthorize(credential)
        .Build<IDemoApi>();

	// 建立代理物件
	var proxy = factory.Create();
    var api = proxy.Api;

    // 呼叫Api
    var ret = await api.Login(new Login { Account = "david", Password = "123" });
```

## 後台API端啟用HAWK驗證 範例 Microsoft.AspNet.WebApi.Owin
- 引用套件 HawkNet.Owin
``` C#
    // Startup 啟動規劃WebApi 與 Hawk驗證
    public class Startup
    {
        public void Configuration(IAppBuilder app) {

            var apiConfig = new System.Web.Http.HttpConfiguration();

            // 啟用Hawk驗證
            {
                var credential = new HawkCredential();
                credential.Id = "API";
                credential.Key = "XXXXXXXX";
                credential.User = "API";
                credential.Algorithm = "sha256";

                app.UseHawkAuthentication(new HawkAuthenticationOptions
                {
                    Credentials = (id) => Task.FromResult(credential),
                    IncludeServerAuthorization = false,
                    TimeskewInSeconds = 120
                });
            }
            // 啟用WebApi
            {
                // use attibute routes
                config.MapHttpAttributeRoutes();

                config.Routes.MapHttpRoute(
                   name: "DefaultApi",
                   routeTemplate: "api/{controller}/{action}/{id}",
                   defaults: new { id = RouteParameter.Optional }
                );

                // 必須套用 DefaultApi Json 設定
                config.Formatters.JsonFormatter.SerializerSettings = DefaultApiExtension.DefaultJsonSerializerSettings;

                //use webapi
                app.UseWebApi(apiConfig);
            }
        }
    }

    /// <summary>
    /// 範例API
    /// </summary>
    [DefaultApiResult] //套用DefaultApi 回傳格式
    [Authorize(Users="API")] // 要求授權通過
    public partial class DemoApiController : ApiController, IDemoApi {
        ...
    }

```

## EasyInjector 整合
``` C#
    var injector = new EasyInjector();

    new ApiProxyBuilder()
        .UseDefaultApiProtocol("http://localhost:5249/api/Demo")
        // 建置整合到注入依賴
        .Build<IDemoApi>(injector);

    using (var scope = injector.CreateScope()) {

        // 取得Proxy
        var proxy1 = scope.ServiceProvider.GetRequiredService<IApiProxy<IDemoApi>>();

        // 取得Api = proxy1.Api
        var api1 = scope.ServiceProvider.GetRequiredService<IDemoApi>();
    }

```