using EasyApiProxys.DemoApis;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;


namespace Tests
{
    /// <summary>
    /// 模擬 Server Api 回應
    /// </summary>
    public class DemoApiServerMockHandler : HttpMessageHandler
    {
        private JsonSerializer _jsonSerailizer;

        public DemoApiServerMockHandler(Func<Newtonsoft.Json.JsonSerializer> jsonSerailizer)
        {
            _jsonSerailizer = jsonSerailizer.Invoke();
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {

            if (request.RequestUri.Segments.Last().Equals("Login", StringComparison.OrdinalIgnoreCase))
            {
                var ret = await Login(request);               
                var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                resp.Content = new ObjectContent<AccountInfo>(ret, new JsonMediaTypeFormatter());
                return resp;
            }
            else if (request.RequestUri.Segments.Last().Equals("Logout", StringComparison.OrdinalIgnoreCase))
            {
                await Logout(request);
                var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK);                
                return resp;
            }
            else if (request.RequestUri.Segments.Last().Equals("GetEmail", StringComparison.OrdinalIgnoreCase))
            {
                var ret = await GetEmail(request);
                var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                resp.Content = new ObjectContent<string>(ret, new JsonMediaTypeFormatter());
                return resp;
            }

            else if (request.RequestUri.Segments.Last().Equals("GetServerInfo", StringComparison.OrdinalIgnoreCase))
            {
                var ret = GetServerInfo();
                var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                resp.Content = new ObjectContent<string>(ret, new JsonMediaTypeFormatter());
                return resp;
            }
            else if (request.RequestUri.Segments.Last().Equals("RunProc_001", StringComparison.OrdinalIgnoreCase))
            {
                var ret = await RunProc_001(request);
                var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                resp.Content = new ObjectContent<string>(ret, new JsonMediaTypeFormatter());
                return resp;
            }
            else
            {
                throw new ApplicationException("Not Found");
            }
        }       

        public async Task<AccountInfo> Login(HttpRequestMessage request)
        {
            await Task.Delay(1000);
            var req = await GetContent<Login>(request);

            if (req.Account == "david" && req.Password == "123")
            {
                return new AccountInfo { Account = req.Account, Token = "123456789", Expired = DateTime.Now.AddHours(1) };
            }
            throw new NotImplementedException();
        }

        public async Task Logout(HttpRequestMessage request)
        {
            await Task.Delay(1000);
            var req = await GetContent<TokenInfo>(request);
            if (req.Token == "123456789")
                return;
            throw new ApplicationException("The Token Not exits");
        }

        public async Task<string> GetEmail(HttpRequestMessage request)
        {
            await Task.Delay(1000);
            var req = await GetContent<TokenInfo>(request);
            if (req.Token == "123456789")
            {
                return "david@gmail.com";
            }
            throw new ApplicationException("The Token Not exits");
        }

        public async Task<string> RunProc_001(HttpRequestMessage request)
        {
            //await Task.Delay(1000);
            var req = await GetContent<ProcInfo>(request);
            if (req.ProcSeconds > 0)
                await Task.Delay(TimeSpan.FromSeconds(req.ProcSeconds));

            return string.Format("OK {0}", req.ProcSeconds);
        }

        public string GetServerInfo()
        {
            return "Demo Server";
        }       

        async Task<T> GetContent<T>(HttpRequestMessage request)
        {
            var s = await request.Content.ReadAsStreamAsync();
            using (var sr = new StreamReader(s))
            using (var jr = new JsonTextReader(sr))
            {
                return _jsonSerailizer.Deserialize<T>(jr);
            }            
        }
    }
}
