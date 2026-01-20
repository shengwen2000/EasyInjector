
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
    [DefaultApiResult(ImStatusCode = 542)]
    [ExceptionStatus<ApplicationException>(503)]
    [ExceptionStatus<NotSupportedException>(525)]
    public class Demo2Controller : ControllerBase, IDemo2Api
    {
        [HttpPost]
        public Task Error1()
        {
            throw new ApiCodeException(ErrorCodes.InvalidAccount, "The Token Not exists");
        }

        [ExceptionStatus<InvalidOperationException>(409)]
        [HttpPost]
        public Task Error2()
        {
            throw new InvalidOperationException("This is an invalid operation exception");
        }

        [HttpPost]
        public Task Error2A()
        {
            throw new ApplicationException("This is an application exception");
        }

        [HttpPost]
        public Task Error2B()
        {
            throw new NotImplementedException("This is a not implemented exception");
        }

        [HttpPost]
        public Task Error2C()
        {
            throw new NotSupportedException("This is a not supported exception");
        }

        [HttpPost]
        [ExceptionStatus<NotSupportedException>(535)]
        public Task Error2D()
        {
            throw new NotSupportedException("This is a not supported exception");
        }

        [HttpPost]
        public Task Error3()
        {
            throw new ValidationException("This is a validation exception");
        }

        [HttpPost]
        public Task Error3A()
        {
            throw new ValidationException("This is a validation exception");
        }

        [DefaultApiResult(ImStatusCode = 543)]
        [HttpPost]
        public Task Error3B()
        {
            throw new ValidationException("This is a validation exception");
        }
    }

    public enum ErrorCodes
    {
        [ApiStatusCode(400)]
        Unknown,
        [ApiStatusCode(401)]
        InvalidAccount,
        [ApiStatusCode(402)]
        InvalidToken,
    }
}
