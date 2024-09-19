
using EasyApiProxys.DemoApis;
using EasyApiProxys.WebApis;
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
        private readonly IDemoApi _api;
        public DemoController(IDemoApi api)
        {
            _api = api;
        }

        [HttpPost]
        public Task<AccountInfo> Login(Login req)
        {
            return _api.Login(req);
        }

        [HttpPost]
        public Task Logout(TokenInfo req)
        {
            return _api.Logout(req);
        }

        [HttpPost]
        public Task<string> GetEmail(TokenInfo req)
        {
            return _api.GetEmail(req);
        }

        [HttpPost]
        public string GetServerInfo()
        {
            return _api.GetServerInfo();
        }

        [HttpPost]
        public Task<string> RunProc(ProcInfo req)
        {
            return _api.RunProc(req);
        }

        [HttpPost]
        public Task<string> RunProc_001(ProcInfo req)
        {
            return _api.RunProc(req);
        }
    }
}
