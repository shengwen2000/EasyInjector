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
        public object GetService(Type serviceType) {

            // 透過 ControllerActivator 建立 ?
            if (serviceType == typeof(System.Web.Mvc.IControllerActivator))
            {
                return new EasyInjectorControllerActivator(_injector);
            }

            // 直接建立
            if (typeof(IController).IsAssignableFrom(serviceType))
            {
                var scope = GetOrCreateScope();
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

        /// <summary>
        /// 確保Rquest Scope 建立與銷毀
        /// </summary>
        private IServiceScope GetOrCreateScope()
        {
            var httpContext = HttpContext.Current;
            if (httpContext == null)
            {
                throw new ApplicationException("HttpContext is not available.");
            }

            // 使用 HttpContext.Items 存儲範圍，確保每個請求獨立        
            var scope = httpContext.Items[EasyInjectorMvcExtension.SCOPE_ITEM_NAME] as IServiceScope;
            if (scope != null)
                return scope;

            scope = _injector.CreateScope();
            httpContext.Items[EasyInjectorMvcExtension.SCOPE_ITEM_NAME] = scope;

            // 在請求結束時處置範圍
            httpContext.AddOnRequestCompleted(ctxEnd =>
            {
                scope.Dispose();
                httpContext.Items.Remove(EasyInjectorMvcExtension.SCOPE_ITEM_NAME);
            });

            return scope;
        }
    }

    
}
