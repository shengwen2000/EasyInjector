
using System.ComponentModel.DataAnnotations;
using EasyApiProxys;
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
    [DefaultApiResult]
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
            DefaultApiResultAttribute.DefaultExStatusCode = 561;
            throw new ArgumentException("This is an argument exception");
        }

        [HttpPost]
        public Task ErrorG3()
        {
            DefaultApiResultAttribute.DefaultExStatusCode = 0;
            throw new ArgumentException("This is an argument exception");
        }
    }


}
