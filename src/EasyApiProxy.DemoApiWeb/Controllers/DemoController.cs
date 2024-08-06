
using EasyApiProxys.DemoApis;
using EasyApiProxys.WebApis;
using KmuApps.Services;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace KmuApps.Controllers
{
    /// <summary>
    /// backendapi
    /// </summary>
    [DefaultApiResult]
    public partial class DemoController : ApiController, IDemoApi
    {
        public DemoController()
        {
        }

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

        [HttpPost]
        public Task<string> RunProc(ProcInfo req)
        {
            return RunProc_001(req);
        }

        [HttpPost]
        public async Task<string> RunProc_001(ProcInfo req)
        {
            if (req.ProcSeconds > 0)
                await Task.Delay(TimeSpan.FromSeconds(req.ProcSeconds));
            return string.Format("OK {0}", req.ProcSeconds);
        }
    }
}
