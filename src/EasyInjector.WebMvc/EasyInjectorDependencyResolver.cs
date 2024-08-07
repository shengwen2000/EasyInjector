using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace EasyInjectors.WebApis
{
    /// <summary>
    /// EasyInjctor for MVC 注入解析器
    /// </summary>
    public class EasyInjectorDependencyResolver : IDependencyResolver
    {
        private readonly EasyInjector _injector;

        /// <summary>
        /// EasyInjctor for MVC 注入解析器
        /// <param name="injector">injecotor</param>
        /// </summary>
        public EasyInjectorDependencyResolver(EasyInjector injector)
        {
            _injector = injector;
        }

        /// <summary>
        /// 取得服務
        /// </summary>
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(System.Web.Mvc.IControllerFactory))
            {
                return new EasyInjectorControllerFactory(_injector);
            }

            // Controller 的話 建立之
            if (typeof(IController).IsAssignableFrom(serviceType))
            {
                var scope = HttpContext.Current.Items[EasyInjectorMvcExtension.SCOPE_ITEM_NAME] as IServiceScope;
                if (scope == null)
                    return null;

                var instance = scope.ServiceProvider.CreateInstance(serviceType);
                return instance;
            }
            return null;
        }

        /// <summary>
        /// 取得服務
        /// </summary>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            var srv = GetService(serviceType);
            if (srv != null)
                yield return srv;
        }
    }

    
}
