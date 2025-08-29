using EasyApiProxys;
using EasyApiProxys.DemoApis;
using EasyApiProxys.WebApis;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Http;

namespace KmuApps.Controllers
{
    /// <summary>
    /// backendapi
    /// </summary>
    [KmuhomeApiResult]
    public partial class DemoController : ApiController, IDemoApi
    {

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
                    Roles = new Roles[] { Roles.AdminUser }
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
        public async Task<string> RunProc(ProcInfo req)
        {
            if (req.ProcSeconds > 0)
                await Task.Delay(TimeSpan.FromSeconds(req.ProcSeconds));
            return string.Format("OK {0}", req.ProcSeconds);
        }

        [HttpPost]
        public Task<string> RunProc_001(ProcInfo req)
        {
            return RunProc(req);
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
            return Task.FromResult(0);
        }

        [Authorize(Users="Admin")]
        [HttpPost]
        public async Task<string> HawkApi()
        {
            await Task.FromResult(0);
            return "hawk api ok";
        }

        [Authorize(Users = "Admin")]
        [HttpPost]
        public async Task<string> BasicApi()
        {
            await Task.FromResult(0);
            return "basic api ok";
        }

        [HttpPost]
        public Task<string> GetBearerToken()
        {
            if (this.Request.Headers.Authorization != null && this.Request.Headers.Authorization.Scheme == "Bearer")
                return Task.FromResult(this.Request.Headers.Authorization.Parameter);
            return Task.FromResult("");
        }

        [HttpPost]
        public void ThrowApiException(DefaultApiResult req)
        {
            throw new ApiCodeException(req.Result, req.Message, req.Data);
        }

        /// <summary>
        /// 產生屬性驗證錯誤
        /// - thorw ValidationException
        /// </summary>
        static public ValidationException GenPropError<T>(T obj, Expression<Func<T, object>> theProp, string errorMessage)
        {

            if (theProp.Body is MemberExpression)
            {
                var mb1 = theProp.Body as MemberExpression;
                var result = new ValidationResult(errorMessage, new string[] { mb1.Member.Name });
                return new ValidationException(result, null, obj);
            }
            else
                throw new ApplicationException("theProp 必須選擇成員屬性");
        }

        [HttpPost]
        public TokenInfo GetTokenInfo()
        {
            return null;
        }
    }
}
