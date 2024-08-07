using EasyInjectors;
using EasyInjectors.WebApis;
using System.Web.Http;

namespace Owin
{
    /// <summary>
    /// EasyInjector OWIN Support
    /// </summary>
    public static class EasyInjectorOwinBuilderExtensions
    {
        /// <summary>
        /// 使用 EasyInjector 作為注入器
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <param name="injector"></param>
        /// <returns></returns>
        public static IAppBuilder UseEasyInjector(this IAppBuilder builder, HttpConfiguration configuration, EasyInjector injector)
        {
            configuration.DependencyResolver = new EasyInjectorResolver(injector);
            return builder;
        }

    }
}

