using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EasyApiProxys.DemoApis
{
    public interface IDemoApi
    {
        Task<AccountInfo> Login(Login req);

        Task Logout(TokenInfo req);

        Task<string> GetEmail(TokenInfo req);

        string GetServerInfo();

        TokenInfo GetTokenInfo();

        [ApiMethod(Name = "RunProc_001", TimeoutSeconds=5)]
        Task<string> RunProc(ProcInfo info);

        /// <summary>
        /// 呼叫會引發一個欄位異常
        /// </summary>
        /// <returns></returns>
        Task RaiseValidateError();

        void NoResult();

        Task NoResult2();

        Task<string> HawkApi();

        Task<string> BasicApi();

        Task<string> GetBearerToken();

        void ThrowApiException(DefaultApiResult req);
    }

    public class Login
    {
        [StringLength(10)]
        public string Account { get; set; }
        [StringLength(10)]
        public string Password { get; set; }
    }

    public class AccountInfo
    {
        public string Account { get; set; }

        public string Token { get; set; }

        public DateTime Expired { get; set; }

        public IEnumerable<Roles> Roles {get; set;}

        public AccountInfo()
        {
            Roles = Enumerable.Empty<Roles>();
        }
    }

    public class TokenInfo
    {
        public string Token { get; set; }
    }

    public class ProcInfo
    {
        public int ProcSeconds { get; set; }
    }

    public enum Roles
    {
        AdminUser,
        User,
        Manager,
    }

}
