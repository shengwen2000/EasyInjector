using System;
using System.Web.Mvc;

namespace EasyInjectors.WebApis
{
    /// <summary>
    /// ControllerActivator
    /// - 負責Controller建立並注入依賴服務  
    /// </summary>
    public class EasyInjectorControllerActivator : IControllerActivator
    {
        private readonly EasyInjector _injector;        

        /// <summary>
        /// Controller Factory 只是用來攔截 Controller的建立與終結事件    
        /// </summary>
        public EasyInjectorControllerActivator(EasyInjector injector)
        {
            _injector = injector;
        }

        /// <summary>
        /// Controller建立與注入依賴服務
        /// </summary>
        public IController Create(System.Web.Routing.RequestContext requestContext, Type controllerType)
        {
            var scope = GetOrCreateScope(requestContext);
            var controller = (scope.ServiceProvider.CreateInstance(controllerType)
                ?? Activator.CreateInstance(controllerType)) as IController;
            if (controller == null)
                throw new ApplicationException(string.Format("無法建立Controller {0}", controllerType.FullName));
            return controller;
        }

        /// <summary>
        /// 確保Rquest Scope 建立與銷毀
        /// </summary>
        private IServiceScope GetOrCreateScope(System.Web.Routing.RequestContext requestContext)
        {
            var httpContext = requestContext.HttpContext;
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
