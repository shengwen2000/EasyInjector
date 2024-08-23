
using EasyApiProxys.DemoApis;
using EasyApiProxys.WebApis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyApiProxy.DemoApiWeb.Controllers
{
    /// <summary>
    /// Demo Api
    /// </summary>
    [ApiController]
    [Route("api/demo")]
    [DefaultApiResult] // 預設API的協定封裝
    [Authorize(AuthenticationSchemes="Hawk", Roles ="Admins")]
    public class DemoController : ControllerBase, IDemoApi
    {
        public DemoController()
        {
        }

        [AllowAnonymous]
        [HttpGet("Ping")]
        public string Ping() {
            return "Hello Ping";
        }

        [HttpPost("Login")]
        public async Task<AccountInfo> Login(Login req)
        {
            await Task.Delay(1000);
            if (req.Account == "david" && req.Password == "123")
            {
                return new AccountInfo { Account = req.Account, Token = "123456789", Expired = DateTime.Now.AddHours(1) };
            }
            throw new NotImplementedException();
        }

        [HttpPost("Logout")]
        public async Task Logout(TokenInfo req)
        {
            await Task.Delay(1000);
            if (req.Token == "123456789")
                return;
            throw new ApplicationException("The Token Not exits");
        }

        [HttpPost("GetEmail")]
        public async Task<string> GetEmail(TokenInfo req)
        {
            await Task.Delay(1000);
            if (req.Token == "123456789")
            {
                return "david@gmail.com";
            }
            throw new ApplicationException("The Token Not exits");
        }

        [HttpPost("GetServerInfo")]
        public string GetServerInfo()
        {
            return "Demo Server";
        }

        [HttpPost("RunProc")]
        public Task<string> RunProc(ProcInfo req)
        {
            return RunProc_001(req);
        }

        [HttpPost("RunProc_001")]
        public async Task<string> RunProc_001(ProcInfo req)
        {
            if (req.ProcSeconds > 0)
                await Task.Delay(TimeSpan.FromSeconds(req.ProcSeconds));
            return string.Format("OK {0}", req.ProcSeconds);
        }
    }
}
