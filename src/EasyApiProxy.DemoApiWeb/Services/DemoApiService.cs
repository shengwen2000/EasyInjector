using EasyApiProxys.DemoApis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmuApps.Services
{
    public class DemoApiService : IDemoApi
    {

        public async Task<AccountInfo> Login(Login req)
        {
            await Task.Delay(1000);
            if (req.Account == "david" && req.Password == "123")
            {
                return new AccountInfo { Account = req.Account, Token = "123456789", Expired = DateTime.Now.AddHours(1) };
            }
            throw new NotImplementedException();
        }


        public async Task Logout(TokenInfo req)
        {
            await Task.Delay(1000);
            if (req.Token == "123456789")
                return;
            throw new ApplicationException("The Token Not exists");
        }


        public async Task<string> GetEmail(TokenInfo req)
        {
            await Task.Delay(1000);
            if (req.Token == "123456789")
            {
                return "david@gmail.com";
            }
            throw new ApplicationException("The Token Not exists");
        }

        public string GetServerInfo()
        {
            return "Demo Server";
        }

        public async Task<string> RunProc(ProcInfo req)
        {
            if (req.ProcSeconds > 0)
                await Task.Delay(TimeSpan.FromSeconds(req.ProcSeconds));
            return string.Format("OK {0}", req.ProcSeconds);
        }
    }
}
