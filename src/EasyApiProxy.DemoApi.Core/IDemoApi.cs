using System.ComponentModel.DataAnnotations;

namespace EasyApiProxys.DemoApis
{
    public interface IDemoApi
    {
        Task<AccountInfo> Login(Login req);

        Task Logout(TokenInfo req);

        Task<string> GetEmail(TokenInfo req);

        string GetServerInfo();

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
        public string Account { get; set; } = default!;
        [StringLength(10)]
        public string Password { get; set; } = default!;
    }

    public class AccountInfo
    {
        public string Account { get; set; } = default!;

        public string Token { get; set; } = default!;

        public DateTime Expired { get; set; }

         public IEnumerable<Roles> Roles {get; set;} = [];
    }

    public class TokenInfo
    {
        public string Token { get; set; } = default!;
    }

    public class ProcInfo
    {
        public int ProcSeconds { get; set; }
    }

    public enum Roles {
        AdminUser,
        User,
        Manager,
    }
}
