
using System.ComponentModel.DataAnnotations;
using EasyApiProxys;
using EasyApiProxys.DemoApis;
using EasyApiProxys.WebApis;
using System.Web.Http;
using System.Threading.Tasks;
using System;
using System.Net;

namespace EasyApiProxy.DemoApiWeb.Controllers
{
    /// <summary>
    /// Demo Api
    /// </summary>
    // 使用Default API 封裝回應
    [KmuhomeApiResult]
    public class Demo3Controller : ApiController, IDemo3Api
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
        public IHttpActionResult IgnoreIt()
        {
            return this.Content(HttpStatusCode.HttpVersionNotSupported, "Ignore It");
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
