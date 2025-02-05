
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using EasyApiProxys;
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
    [Route("api/[controller]/[action]")]
    // 使用 KmuhomeApi 封裝回應
    [KmuhomeApiResult]
    public class DemoController : ControllerBase, IDemoApi
    {
        public DemoController()
        {
        }

        [HttpGet]
        public string Ping()
        {
            return "Hello Ping";
        }

        [HttpPost]
        public async Task<AccountInfo> Login(Login req)
        {
            await Task.Delay(1000);
            if (req.Account == "david" && req.Password == "123")
            {
                return new AccountInfo
                {
                    Account = req.Account,
                    Token = "123456789",
                    Expired = DateTime.Now.AddHours(1),
                    Roles = [Roles.AdminUser]
                };
            }
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task Logout(TokenInfo req)
        {
            await Task.Delay(1000);
            if (req.Token == "123456789")
                return;
            throw new ApplicationException("The Token Not exists");
        }

        [HttpPost]
        public async Task<string> GetEmail(TokenInfo req)
        {
            await Task.Delay(1000);
            if (req.Token == "123456789")
            {
                return "david@gmail.com";
            }
            throw new ApplicationException("The Token Not exists");
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

        [HttpPost]
        public Task RaiseValidateError()
        {
            var info = new AccountInfo();
            throw GenPropError(info, o => o.Account, "帳號為必要");
        }

        [HttpPost]
        public void NoResult()
        {
        }

        [HttpPost]
        public Task NoResult2()
        {
            return Task.CompletedTask;
        }

        [Authorize(AuthenticationSchemes = "Hawk", Roles = "Admins")]
        [HttpPost]
        public async Task<string> HawkApi()
        {
            await Task.FromResult(0);
            return "hawk api ok";
        }

         /// <summary>
        /// 產生屬性驗證錯誤
        /// - thorw ValidationException
        /// </summary>
        static public ValidationException GenPropError<T>(T obj, Expression<Func<T, object?>> theProp, string? errorMessage) {

            if(theProp.Body is MemberExpression mb1) {
                var result = new ValidationResult(errorMessage, [mb1.Member.Name]);
                return new ValidationException(result, null, obj);
            }
            else
                throw new ApplicationException($"{nameof(theProp)} 必須選擇成員屬性");
        }
    }
}
