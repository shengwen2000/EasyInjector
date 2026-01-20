
using System.ComponentModel.DataAnnotations;
using EasyApiProxys.DemoApis;
using EasyApiProxys.WebApis;
using Microsoft.AspNetCore.Mvc;

namespace EasyApiProxy.DemoApiWeb.Controllers
{
    /// <summary>
    /// Demo Api
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    // 使用Default API 封裝回應
    [KmuhomeApiResult]
    public class Demo3Controller : ControllerBase, IDemo3Api
    {

        [HttpPost]
        public Task ErrorG1()
        {
            throw new ValidationException("This is a validation exception");
        }

        [HttpPost]
        public Task ErrorG2()
        {
            KmuhomeApiResultAttribute.DefaultExStatusCode = 561;
            throw new ArgumentException("This is an argument exception");
        }

        [HttpPost]
        public Task ErrorG3()
        {
            KmuhomeApiResultAttribute.DefaultExStatusCode = 0;
            throw new ArgumentException("This is an argument exception");
        }

        [HttpPost]
        public string IgnoreIt()
        {
            Response.StatusCode = 571;
            return "Ignore It";
        }

        [HttpPost]
        public int LegacyHeaderEnabled()
        {
            KmuhomeApiResultAttribute.CompatibleLegacyHeader = true;
            return 2;
        }

        [HttpPost]
        public int LegacyHeaderDisabled()
        {
            KmuhomeApiResultAttribute.CompatibleLegacyHeader = false;
            return 1;
        }
    }
}
