using EasyApiProxys.DemoApis;
using EasyInjectors.Dev;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmuApps.Services
{
    public class DemoApiServiceDev : IDemoApi
    {
        private readonly IDemoApi _baseone;

        public DemoApiServiceDev(IDemoApi baseone)
        {
            _baseone = baseone;
        }

        public Task<AccountInfo> Login(Login req)
        {
            throw new NotImplementedException();
        }

        public Task Logout(TokenInfo req)
        {
            throw new NotImplementedException();
        }

        [Override]
        public Task<string> GetEmail(TokenInfo req)
        {
            return _baseone.GetEmail(req);
        }

        public string GetServerInfo()
        {
            throw new NotImplementedException();
        }

        public Task<string> RunProc(ProcInfo info)
        {
            throw new NotImplementedException();
        }
    }
}
