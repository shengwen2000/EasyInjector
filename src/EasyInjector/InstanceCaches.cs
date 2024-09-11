using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyInjectors
{
    /// <summary>
    /// 負責存放服務實例快取
    /// Singleton EasyInjector 有一個 Cache
    /// Scope 則是每一個 Scope實例都內建一個 Cache
    /// </summary>
    class InstanceCaches : IDisposable
    {
        /// <summary>
        /// 一般實例存放處
        /// </summary>
        Dictionary<ServiceRegister, object> _instances = new Dictionary<ServiceRegister, object>();

        /// <summary>
        /// 泛型實例存放處
        /// </summary>
        Dictionary<ServiceRegister, Dictionary<Type, object>> _instances_generic = new Dictionary<ServiceRegister, Dictionary<Type, object>>();

        private bool disposed = false;

        /// <summary>
        /// 取得或建立服務
        /// </summary>
        /// <param name="provider">服務提供者</param>
        /// <param name="register">服務註冊</param>
        /// <param name="serviceType">要求服務類型</param>
        /// <returns></returns>
        public object GetOrCreateInstance(IServiceProvider provider, ServiceRegister register, Type serviceType)
        {
            // 一般服務
            if (register.IsGeneric == false)
            {
                object instance;
                if (_instances.TryGetValue(register, out instance) == false)
                {
                    instance = register.CreateFunc(provider);
                    _instances.Add(register, instance);
                }
                return instance;
            }
            // 泛型服務(每個泛型服務都有自己專屬(sericetype, instance)
            else
            {
                //找出自己專屬的Instancss
                Dictionary<Type, object> instances;
                object instance;
                if (_instances_generic.TryGetValue(register, out instances) == false)
                {
                    instances = new Dictionary<Type, object>();
                    _instances_generic.Add(register, instances);
                }

                // 不存在就新增一個
                if (instances.TryGetValue(serviceType, out instance) == false)
                {
                    var genericArgs = serviceType.GetGenericArguments();
                    instance = register.CreateGenericFunc(provider, genericArgs);
                    instances.Add(serviceType, instance);
                }
                return instance;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            // 先說已經disposed 避免底下可能呼叫到內含自身服務的Dispose()形成無限迴圈
            disposed = true;

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
        }
    }
}
