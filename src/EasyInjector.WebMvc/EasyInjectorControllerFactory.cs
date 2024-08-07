using System;
using System.Web;
using System.Web.Mvc;

namespace EasyInjectors.WebApis
{
    /// <summary>
    /// Controller Factory 只是用來攔截 Controller的建立與終結事件    
    /// </summary>
    public class EasyInjectorControllerFactory : DefaultControllerFactory
    {
        private readonly EasyInjector _injector;

        /// <summary>
        /// Controller Factory 只是用來攔截 Controller的建立與終結事件    
        /// </summary>
        public EasyInjectorControllerFactory(EasyInjector injector)
        {
            _injector = injector;
        }

        /// <summary>
        /// 建立Controller
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        protected override IController GetControllerInstance(System.Web.Routing.RequestContext requestContext, Type controllerType)
        {
            var scope = _injector.CreateScope();
            requestContext.HttpContext.Items.Add("myScope", scope);
            return base.GetControllerInstance(requestContext, controllerType);
        }

        /// <summary>
        /// 終結Controller
        /// </summary>
        /// <param name="controller"></param>
        public override void ReleaseController(IController controller)
        {
            base.ReleaseController(controller);
            var scope = HttpContext.Current.Items["myScope"] as IServiceScope;           
            if (scope != null)
                scope.Dispose();
        }
    }
}
