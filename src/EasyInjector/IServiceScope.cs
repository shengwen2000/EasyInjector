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
    class CScope : IServiceScope, IServiceProvider
    {
        /// <summary>
        /// Scope 實例服務的存放位置
        /// </summary>
        internal Dictionary<ServiceRegister, object> _instances = new Dictionary<ServiceRegister, object>();

        /// <summary>
        /// Scope 實例服務的存放位置(泛型)
        /// </summary>
        internal Dictionary<ServiceRegister, Dictionary<Type, object>> _instances_generic = new Dictionary<ServiceRegister, Dictionary<Type, object>>();

        private EasyInjector _simpleService;

        private bool disposed = false;

        public CScope(EasyInjector simpleService)
        {
            _simpleService = simpleService;           
        }

        ~CScope()
        {
            Dispose(false);
        }

        public object GetService(Type serviceType)
        {
            return _simpleService.GetService(serviceType, this);
        }

        public IServiceProvider ServiceProvider
        {
            get { return this; }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            //正常Dispose，所有子項目一併施放
            if (disposing)
            {
                //一般實例
                foreach (var kv in _instances)
                {
                    var instance = kv.Value as IDisposable;
                    if (instance != null)
                    {
                        try { instance.Dispose(); }
                        catch { }
                    }
                }
                _instances.Clear();

                //泛型實例
                foreach (var kv1 in _instances_generic)
                {
                    foreach (var obj in kv1.Value.Values)
                    {
                        var instance = obj as IDisposable;
                        if (instance != null)
                        {
                            try { instance.Dispose(); }
                            catch { }
                        }
                    }
                    kv1.Value.Clear();
                }
                _instances_generic.Clear();
            }
            //不正常Dispose只要確保自身資源釋放即可
            else
            {
            }
            disposed = true;
        }
    }
}
