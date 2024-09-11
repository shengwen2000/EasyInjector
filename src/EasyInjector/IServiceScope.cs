using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyInjectors
{
    /// <summary>
    /// 請參考 .Net Core的定義
    /// </summary>
    public interface IServiceScope : IDisposable
    {
        /// <summary>
        /// 服務提供者
        /// </summary>
        IServiceProvider ServiceProvider { get; }
    }

    /// <summary>
    /// 提供Scope支援
    /// </summary>
    public class ServiceScope : IServiceScope, IServiceProvider
    {
        /// <summary>
        /// Scope實例儲放位置
        /// </summary>
        internal InstanceCaches _caches = new InstanceCaches();

        private EasyInjector _injector;

        private bool disposed = false;

        /// <summary>
        /// Scope範圍
        /// </summary>
        public ServiceScope(EasyInjector injector)
        {
            _injector = injector;           
        }

        /// <summary>
        /// 解構
        /// </summary>
        ~ServiceScope()
        {
            Dispose(false);
        }

        /// <summary>
        /// 取得服務
        /// </summary>
        public object GetService(Type serviceType)
        {
            return _injector.GetService(serviceType, this);
        }

        /// <summary>
        /// 服務提供者
        /// </summary>
        public IServiceProvider ServiceProvider
        {
            get { return this; }
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
                _caches.Dispose();
            }
            //不正常Dispose只要確保自身資源釋放即可
            else
            {
            }
        }
    }
}
