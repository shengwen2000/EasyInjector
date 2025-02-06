using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace KmuApps.Services
{
    /// <summary>
    /// 代表Console版本的OWIN服務
    /// </summary>
    public interface IOwinWebService
    {
        /// <summary>
        /// 啟動服務
        /// </summary>
        Task StartAsync(CancellationToken ctoken);
    }

    public class OwinWebService : IOwinWebService
    {
        //private Thread _thread;
        private CancellationToken _ctoken;
        private Task _task1;
        private readonly Startup _startup;
        private volatile IDisposable _webapp;

        public OwinWebService(Startup startup)
        {          
            _startup = startup;
        }

        public async Task StartAsync(CancellationToken ctoken)
        {
            if (_webapp == null)
            {
                _ctoken = ctoken;
                _task1 = Task.Run(() => WebEntry());
                await _task1;
            }
        }

        void WebEntry()
        {
            _webapp = Microsoft.Owin.Hosting.WebApp.Start(
                   new Microsoft.Owin.Hosting.StartOptions("http://localhost:5249"),
                   appbuilder => _startup.Configuration(appbuilder));
        }
    }
}
