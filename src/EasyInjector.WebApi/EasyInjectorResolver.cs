using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dependencies;

namespace EasyInjectors.WebApis
{
    /// <summary>
    /// EasyInjector for web api 解析注入器實作
    /// </summary>
    public class EasyInjectorResolver : IDependencyResolver
    {
        private EasyInjector _injector;

        private bool disposed = false;

        /// <summary>
        /// EasyInjector for web api 解析注入器實作
        /// <param name="injector"></param>
        /// </summary>
        public EasyInjectorResolver(EasyInjector injector)
        {
            _injector = injector;
        }

        /// <summary>
        /// DeConstructor
        /// </summary>
        ~EasyInjectorResolver()
        {
            Dispose(false);
        }


        /// <summary>
        /// BeginScope
        /// </summary>
        public IDependencyScope BeginScope()
        {
            var scope1 = _injector.CreateScope();
            return new CScope(scope1);
        }

        /// <summary>
        /// 取得服務
        /// </summary>
        /// <param name="serviceType"></param>
        public object GetService(Type serviceType)
        {
            return _injector.GetService(serviceType);
        }

        /// <summary>
        /// 取得服務
        /// </summary>
        /// <param name="serviceType"></param>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            var srv1 = _injector.GetService(serviceType);
            if (srv1 != null) yield return srv1;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            // 先說已經disposed 避免底下可能呼叫到內含自身服務的Dispose()形成無限迴圈
            disposed = true;

            //正常Dispose，所有子項目一併施放
            if (disposing)
            {
                _injector.Dispose();
            }            
        }

        /// <summary>
        /// 實作符合 IDependencyScope的規範
        /// </summary>
        class CScope : IDependencyScope
        {
            private IServiceScope _scope;

            public CScope(IServiceScope scope)
            {
                _scope = scope;
            }

            public object GetService(Type serviceType)
            {
                if (typeof(ApiController).IsAssignableFrom(serviceType))
                {
                    return _scope.ServiceProvider.CreateInstance(serviceType);
                }

                return _scope.ServiceProvider.GetService(serviceType);
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                var srv1 = GetService(serviceType);
                if (srv1 != null) yield return srv1;
            }

            public void Dispose()
            {
                _scope.Dispose();
            }
        }
    }
}
