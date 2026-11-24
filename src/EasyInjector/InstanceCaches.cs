using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<ServiceRegister, object> _instances = new ConcurrentDictionary<ServiceRegister, object>();

        /// <summary>
        /// 泛型實例存放處
        /// </summary>
        private readonly ConcurrentDictionary<ServiceRegister, ConcurrentDictionary<Type, object>> _instances_generic = new ConcurrentDictionary<ServiceRegister, ConcurrentDictionary<Type, object>>();

        private readonly object _lock = new object();

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
                if (_instances.TryGetValue(register, out instance))
                {
                    return instance;
                }

                lock (_lock)
                {
                    if (_instances.TryGetValue(register, out instance) == false)
                    {
                        instance = register.CreateFunc(provider);
                        _instances[register] = instance;
                    }
                    return instance;
                }
            }
            // 泛型服務(每個泛型服務都有自己專屬(sericetype, instance)
            else
            {
                //找出自己專屬的Instances
                var instances = _instances_generic.GetOrAdd(register, _ => new ConcurrentDictionary<Type, object>());

                object instance;
                if (instances.TryGetValue(serviceType, out instance))
                {
                    return instance;
                }

                lock (_lock)
                {
                    // 不存在就新增一個
                    if (instances.TryGetValue(serviceType, out instance) == false)
                    {
                        var genericArgs = serviceType.GetGenericArguments();
                        instance = register.CreateGenericFunc(provider, genericArgs);
                        instances[serviceType] = instance;
                    }
                    return instance;
                }
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
