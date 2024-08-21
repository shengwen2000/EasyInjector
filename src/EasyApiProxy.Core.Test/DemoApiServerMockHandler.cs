using EasyApiProxys.DemoApis;
using Newtonsoft.Json;
using System.Net.Http.Json;


namespace Tests
{
    /// <summary>
    /// 模擬 Server Api 回應
    /// </summary>
    public class DemoApiServerMockHandler(Func<JsonSerializer> jsonSerailizer) : HttpMessageHandler
    {
        private JsonSerializer _jsonSerailizer = jsonSerailizer.Invoke();

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var ret1 = await Task.Run(async () => {
                if (request.RequestUri!.Segments.Last().Equals("Login", StringComparison.OrdinalIgnoreCase))
                {
                    var ret = await Login(request).ConfigureAwait(false);
                    var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(ret)
                    };
                    return resp;
                }
                else if (request.RequestUri.Segments.Last().Equals("Logout", StringComparison.OrdinalIgnoreCase))
                {
                    await Logout(request).ConfigureAwait(false);
                    var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                    return resp;
                }
                else if (request.RequestUri.Segments.Last().Equals("GetEmail", StringComparison.OrdinalIgnoreCase))
                {
                    var ret = await GetEmail(request).ConfigureAwait(false);
                    var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(ret)
                    };
                    return resp;
                }

                else if (request.RequestUri.Segments.Last().Equals("GetServerInfo", StringComparison.OrdinalIgnoreCase))
                {
                    var ret = GetServerInfo();
                    var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(ret)
                    };
                    return resp;
                }
                else if (request.RequestUri.Segments.Last().Equals("RunProc_001", StringComparison.OrdinalIgnoreCase))
                {
                    var ret = await RunProc_001(request, cancellationToken).ConfigureAwait(false);
                    var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(ret)
                    };
                    return resp;
                }
                else
                {
                    throw new ApplicationException("Not Found");
                }

            }, cancellationToken).ConfigureAwait(false);

            return ret1;
        }

        public async Task<AccountInfo> Login(HttpRequestMessage request)
        {
            await Task.Delay(1000).ConfigureAwait(false);
            var req = await GetContent<Login>(request).ConfigureAwait(false);

            if (req.Account == "david" && req.Password == "123")
            {
                return new AccountInfo { Account = req.Account, Token = "123456789", Expired = DateTime.Now.AddHours(1) };
            }
            throw new NotImplementedException();
        }

        public async Task Logout(HttpRequestMessage request)
        {
            await Task.Delay(1000).ConfigureAwait(false);
            var req = await GetContent<TokenInfo>(request).ConfigureAwait(false);
            if (req.Token == "123456789")
                return;
            throw new ApplicationException("The Token Not exits");
        }

        public async Task<string> GetEmail(HttpRequestMessage request)
        {
            await Task.Delay(1000).ConfigureAwait(false);
            var req = await GetContent<TokenInfo>(request).ConfigureAwait(false);
            if (req.Token == "123456789")
            {
                return "david@gmail.com";
            }
            throw new ApplicationException("The Token Not exits");
        }

        public async Task<string> RunProc_001(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //await Task.Delay(1000).ConfigureAwait(false);
            var req = await GetContent<ProcInfo>(request).ConfigureAwait(false);
            if (req.ProcSeconds > 0)
                await Task.Delay(TimeSpan.FromSeconds(req.ProcSeconds), cancellationToken);

            return string.Format("OK {0}", req.ProcSeconds);
        }

        public string GetServerInfo()
        {
            return "Demo Server";
        }

        async Task<T> GetContent<T>(HttpRequestMessage request)
        {
            var s = await request.Content!.ReadAsStreamAsync().ConfigureAwait(false);
            using var sr = new StreamReader(s);
            using var jr = new JsonTextReader(sr);
            return _jsonSerailizer.Deserialize<T>(jr)!;
        }
    }
}
