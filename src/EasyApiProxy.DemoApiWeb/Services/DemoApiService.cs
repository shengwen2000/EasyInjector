using EasyApiProxys.DemoApis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
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
                return new AccountInfo { 
                    Account = req.Account, 
                    Token = "123456789", 
                    Expired = DateTime.Now.AddHours(1),
                    Roles = new Roles[] { Roles.AdminUser }
                };
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


        public Task RaiseValidateError()
        {
            var info = new AccountInfo();
            throw GenPropError(info, o => o.Account, "帳號為必要");
        }

        public void NoResult()
        {
            
        }

        public Task NoResult2()
        {
            return Task.FromResult(0);
        }

        public async Task<string> HawkApi()
        {
            await Task.FromResult(0);
            return "hawk api ok";
        }

         /// <summary>
        /// 產生屬性驗證錯誤
        /// - thorw ValidationException
        /// </summary>
        static public ValidationException GenPropError<T>(T obj, Expression<Func<T, object>> theProp, string errorMessage) {

            if(theProp.Body is MemberExpression) {
                var mb1 = theProp.Body as MemberExpression;
                var result = new ValidationResult(errorMessage, new string[] { mb1.Member.Name });
                return new ValidationException(result, null, obj);
            }
            else
                throw new ApplicationException("theProp 必須選擇成員屬性");
        }
    }
}
