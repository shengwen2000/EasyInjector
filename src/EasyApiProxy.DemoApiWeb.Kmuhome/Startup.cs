using EasyApiProxys;
using EasyApiProxys.DemoApis;
using EasyInjectors;
using HawkNet;
using HawkNet.Owin;
using KmuApps.Services;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Logging;
using Owin;
using System.Threading.Tasks;
using System.Web.Http;
//[assembly: OwinStartup(typeof(KMUH.ServiceDiagTool.UI.MVC.Startup))]

namespace KmuApps
{
    /// <summary>
    /// 啟動網站
    /// </summary>
    public class Startup
    {
        static HawkCredential _admin = new HawkCredential
        {
            Id = "123",
            Key = "werxhqb98rpaxn39848xrunpaw3489ruxnpa98w4rxn",
            Algorithm = "sha256",
            User = "Admin",
        };

        /// <summary>
        /// Auto Run by Owin 
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            var logger = app.CreateLogger("WebApp");

            // add http logging
            app.Use(async (ctx, next) =>
            {
                await next();
                logger.WriteInformation(string.Format("HTTP>{0} {1} Status={2}", ctx.Request.Method, ctx.Request.Path, ctx.Response.StatusCode));
            });

            var injector = new EasyInjector();
            injector.AddSingleton<IDemoApi, DemoApiService>();

            // hawk authoreize
            app.UseHawkAuthentication(new HawkAuthenticationOptions
            {
                Credentials = Credentials,
                IncludeServerAuthorization = false,
                TimeskewInSeconds = 60
            });

            // webapi register
            {
                var config = new System.Web.Http.HttpConfiguration();

                app.UseEasyInjector(config, injector);

                // use attibute routes
                config.MapHttpAttributeRoutes();

                // json serialize setting
                config.Formatters.JsonFormatter.SerializerSettings = KmuhomeApiExtension.DefaultJsonSerializerSettings;

                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{action}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );

                //use webapi
                app.UseWebApi(config);
            }

            app.UseCors(CorsOptions.AllowAll);
            //app.MapSignalR();
        }

        async Task<HawkNet.HawkCredential> Credentials(string userId)
        {
            await Task.FromResult(0);

            if (userId == _admin.Id)
                return _admin;
            return null;
        }
    }
}
