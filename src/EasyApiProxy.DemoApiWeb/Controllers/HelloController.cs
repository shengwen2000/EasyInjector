
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
    public partial class HelloController : ApiController
    {
        private readonly IHelloService _hello;

        public HelloController(IHelloService hello)
        {
            _hello = hello;
        }

        [HttpPost]
        public async Task<string> SayHello()
        {
            await Task.Delay(1000);
            return _hello.SayHello();
        }
    }
}
