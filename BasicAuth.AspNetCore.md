# Basic.AspNetCore

* 支援 asp.net core 8 提供 Basic驗證。


## 註冊Basic驗證
``` C#
 // 授權服務註冊
builder.Services.AddAuthentication()
    // 註冊Basic驗證
    .AddBasicAuthorize(opt =>
    {
        // 設定證書
        opt.Credentials.Add(new BasicAuth.AspNetCore.BasicCredential
        {
            Account = "admin",
            PassCode = "admin1234",
            User = "Admin",
            Roles = [ "Admins" ]
        });

		// 也可以自行實作取得證書方法
        //opt.GetCredentialFunc = (acct) => Task.FromResult(new BasicAuth.AspNetCore.BasicCredential());
    });
```

## 啟用授權驗證
``` C#
	// 啟用憑證檢驗 確定憑證有效性
	app.UseAuthentication();
	// 啟用權限驗證機制 檢查你是否有權限存取方法
	app.UseAuthorization();
```

## ApiController 套用 Basic驗證範例
``` C#
    [ApiController]
    [Route("api/demo")]
    [Authorize(AuthenticationSchemes="Basic", Roles ="Admins")]
    public class DemoController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("Ping")]
        public string Ping() {
            return "Hello Ping AllowAnonymous";
        }

        [HttpGet("Hello")]
        public string Hello() {
            return "Hello Basic Auth Success";
        }
	}
```