# HawkNet.AspNetCore

* 支援 asp.net core 8 提供 Hawk驗證。
* Hawk 核心函數移植自 https://github.com/pcibraro/hawknet
	- 因原作者很久沒有維護了，自行移植比較保險
* 性能考量 暫不提供 Hawk 內容Hash驗證的部分。

## 註冊HAWK驗證
``` C#
 // 授權服務註冊
builder.Services.AddAuthentication()
    // 註冊Hawk驗證
    .AddHawkAuthorize(opt =>
    {
        // 設定證書
        opt.Credentials.Add(new HawkNet.AspNetCore.HawkCredential
        {
            Id = "123",
            Key = "werxhqb98rpaxn39848xrunpaw3489ruxnpa98w4rxn",
            Algorithm = "sha256",
            User = "Admin",
            Roles= ["Admins"]
        });

		// 也可以自行實作取得證書方法
        //opt.GetCredentialFunc = (id) => Task.FromResult(new HawkNet.AspNetCore.HawkCredential());
    });
```

## 啟用授權驗證
``` C#
	// 啟用憑證檢驗 確定憑證有效性
	app.UseAuthentication();
	// 啟用權限驗證機制 檢查你是否有權限存取方法
	app.UseAuthorization();
```

## ApiController 套用 Hawk驗證範例
``` C#
    [ApiController]
    [Route("api/demo")]
    [Authorize(AuthenticationSchemes="Hawk", Roles ="Admins")]
    public class DemoController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("Ping")]
        public string Ping() {
            return "Hello Ping AllowAnonymous";
        }

        [HttpGet("Hello")]
        public string Hello() {
            return "Hello Hawk Success";
        }
	}
```