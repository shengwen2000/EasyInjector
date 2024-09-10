# EasyApiProxy

* 簡單的 Web Api 代理類別產生
* 支援Hawk驗證
* [Nuget Package Install](https://www.nuget.org/packages/EasyApiProxy/)
* [詳細內容](./EasyApiProxy.md)
* 提供 DefaultApi 實作支援 於套件 EasyApiProxy.WebApi
* 提供 用戶端 Hawk驗證 於套件 EasyApiProxy.HawkAuth

## 呼叫範例
``` C#
    // 建立API 代理物件 Factory 必須重複使用 因為其實做包含一個專用的 HttpClient
    // HttpClinet 微軟官方明確的說必須公用 否則回有tcp/ip資源不足的問題
    var factory = new ApiProxyBuilder()
        // 預設的Api Protocol 就是異常錯誤如何封裝的方式
        .UseDefaultApiProtocol("http://localhost:8081/api/Demo")
        .Build<IDemoApi>();

	// 建立代理物件
	var proxy = factory.Create();

    // 呼叫Api
    var ret = await proxy.Login(new Login { Account = "david", Password = "123" });
```

## Api 介面定義
``` C#
	// support async
    public interface IDemoApi
    {
        Task<AccountInfo> Login(Login req);

        Task Logout(TokenInfo req);

        Task<string> GetEmail(TokenInfo req);

        string GetServerInfo();
    }
```

## 後台實作範例
- 參考套件 EasyApiProxy.WebApi 提供 DefaultApiResult 實作
``` C#
    /// <summary>
    /// backendapi
    /// </summary>
    [DefaultApiResult]
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
            throw new ApplicationException("The Token Not exits");
        }

        [HttpPost]
        public async Task<string> GetEmail(TokenInfo req)
        {
            await Task.Delay(1000);
            if (req.Token == "123456789")
            {
                return "david@gmail.com";
            }
            throw new ApplicationException("The Token Not exits");
        }

        [HttpPost]
        public string GetServerInfo()
        {
            return "Demo Server";
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
        .UseDefaultApiProtocol("http://localhost:8081/api/Demo")
        // 啟用Hawk驗證
        .UseHawkAuthorize(credential)
        .Build<IDemoApi>();

	// 建立代理物件
	var proxy = factory.Create();

    // 呼叫Api
    var ret = await proxy.Login(new Login { Account = "david", Password = "123" });
```

## 後台API端啟用HAWK驗證 範例
- 引用套件 HawkNet.Owin
``` C#
    public class Startup
    {
        public void Configuration(IAppBuilder app) {

            var apiConfig = new System.Web.Http.HttpConfiguration();

             // use api 預設路由
            apiConfig.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // use hawk auth
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

            //use webapi
            app.UseWebApi(apiConfig);
        }
    }

     /// <summary>
    /// backendapi
    /// </summary>
    [DefaultApiResult]
    // 要求授權通過
    [Authorize(Users="API")]
    public partial class DemoApiController : ApiController, IDemoApi {
        ...
    }

```