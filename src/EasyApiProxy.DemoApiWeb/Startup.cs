using EasyApiProxys;
using EasyApiProxys.DemoApis;
using EasyInjectors;
using KmuApps.Services;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using System.IO;
using System.Web.Http;
//[assembly: OwinStartup(typeof(KMUH.ServiceDiagTool.UI.MVC.Startup))]

namespace KmuApps
{
    /// <summary>
    /// 啟動網站
    /// </summary>
    public class Startup
    {
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
            injector.AddSingleton<IHelloService, HelloService>();
            injector.AddSingleton<IDemoApi, DemoApiService>();
            injector.AddOverride<IDemoApi, DemoApiServiceDev>();

            // webapi register
            {              
                var config = new System.Web.Http.HttpConfiguration();

                app.UseEasyInjector(config, injector);

                // use attibute routes
                config.MapHttpAttributeRoutes();

                // json serialize setting
                config.Formatters.JsonFormatter.SerializerSettings = DefaultApiExtension.DefaultJsonSerializerSettings;

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
    }
}
