# EasyApiProxy

* 簡單的 Web Api 代理類別產生
* 支援Hawk驗證
* [Nuget Package Install](https://www.nuget.org/packages/EasyApiProxy/)
* [詳細內容](./EasyApiProxy.md)


## 呼叫範例
``` C#
    // 建立API 代理物件 可以公用 也必須公用 因為其實做包含一個專用的 HttpClient
    // HttpClinet 微軟官方明確的說必須公用 否則回有tcp/ip資源不足的問題
    var api = new ApiProxyBuilder()
        // 預設的Api Protocol 就是異常錯誤如何封裝的方式
        .UseDefaultApiProtocol("http://localhost:8081/api/Demo")
        .Build<IDemoApi>();

    // 呼叫Api
    var ret = await api.Login(new Login { Account = "david", Password = "123" });
```

## Api 介面定義
``` C#
    public interface IDemoApi
    {
        Task<AccountInfo> Login(Login req);

        Task Logout(TokenInfo req);

        Task<string> GetEmail(TokenInfo req);

        string GetServerInfo();

        [ApiMethod(Name = "RunProc_001")]
        Task<string> RunProc(ProcInfo info);
    }
```

## 後台實作範例
``` C#
    /// <summary>
    /// backendapi
    /// </summary>
    [DefaultApiResult]
    [RoutePrefix("api/Demo")]
    public partial class DemoApiController : ApiController, IDemoApi
    {
        [Route("Login"), HttpPost]
        public async Task<AccountInfo> Login(Login req)
        {
            await Task.Delay(1000);
            if (req.Account == "david" && req.Password == "123")
            {
                return new AccountInfo { Account = req.Account, Token = "123456789", Expired = DateTime.Now.AddHours(1) };
            }
            throw new NotImplementedException();
        }

        [Route("Logout"), HttpPost]
        public async Task Logout(TokenInfo req)
        {
            await Task.Delay(1000);
            if (req.Token == "123456789")
                return;
            throw new ApplicationException("The Token Not exits");
        }

        [Route("GetEmail"), HttpPost]
        public async Task<string> GetEmail(TokenInfo req)
        {
            await Task.Delay(1000);
            if (req.Token == "123456789")
            {
                return "david@gmail.com";
            }
            throw new ApplicationException("The Token Not exits");
        }

        [Route("GetServerInfo"), HttpPost]
        public string GetServerInfo()
        {
            return "Demo Server";
        }
    }
```