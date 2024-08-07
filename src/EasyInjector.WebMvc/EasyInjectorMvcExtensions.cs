using EasyInjectors.WebApis;
using System.Web.Mvc;

namespace EasyInjectors
{
    /// <summary>
    /// EasyInjector for support MVC Controller
    /// </summary>
    public static class EasyInjectorMvcExtension
    {
        internal static string SCOPE_ITEM_NAME = "EasyInjectorScope";

        /// <summary>
        /// 使用 EasyInjector 作為注入器
        /// </summary>      
        public static void UseEasyInjector(EasyInjector injector)
        {
            DependencyResolver.SetResolver(new EasyInjectorDependencyResolver(injector));
        }
    }
}

