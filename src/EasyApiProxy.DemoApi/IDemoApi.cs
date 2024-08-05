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

        [ApiMethod(Name = "RunProc_001")]
        Task<string> RunProc(ProcInfo info);
    }

    public class Login
    {      
        public string Account { get; set; }
        public string Password { get; set; }
    }

    public class AccountInfo
    {
        public string Account { get; set; }

        public string Token { get; set; }

        public DateTime Expired { get; set; }

    }

    public class TokenInfo
    {
        public string Token { get; set; }
    }

    public class ProcInfo
    {
        public int ProcSeconds { get; set; }        
    }

   
}
