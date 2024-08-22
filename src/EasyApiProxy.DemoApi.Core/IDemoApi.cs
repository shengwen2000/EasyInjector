using System;
using System.Threading.Tasks;
using EasyApiProxys;

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
    }

    public class Login
    {
        public string Account { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class AccountInfo
    {
        public string Account { get; set; } = default!;

        public string Token { get; set; } = default!;

        public DateTime Expired { get; set; }

    }

    public class TokenInfo
    {
        public string Token { get; set; } = default!;
    }

    public class ProcInfo
    {
        public int ProcSeconds { get; set; }
    }
}
