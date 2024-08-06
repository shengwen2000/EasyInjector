
using KmuApps.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KmuApps
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            var cts = new System.Threading.CancellationTokenSource();
            var startup = new Startup();
            var owin = new OwinWebService(startup);
            var owintask = owin.StartAsync(cts.Token);
            Console.WriteLine("Web Start Press any key to exit");
            Console.ReadKey();
            cts.Cancel();
            return 0;
        }
    }
}
