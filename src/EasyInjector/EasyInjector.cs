

using System;
using System.Linq;
using System.Collections.Generic;

namespace EasyInjectors
{
#pragma warning disable 1591

    /// <summary>
    /// 簡單的DI容器
    /// </summary>
    public class EasyInjector : IServiceProvider, IServiceScopeFactory, IEnumerable<ServiceRegister>, IDisposable
    {
        /// <summary>
        /// 服務註冊紀錄
        /// </summary>
        private readonly Dictionary<Type, ServiceRegister> _serviceRegisters = new Dictionary<Type, ServiceRegister>(100);

        private readonly object _serviceRegisters_lock = new object();

        private bool disposed = false;

        /// <summary>
        /// 單一實例服務的存放位置
        /// </summary>
        Dictionary<ServiceRegister, object> _instances = new Dictionary<ServiceRegister, object>();

        /// <summary>
        /// 單一實例服務的存放位置(泛型)
        /// </summary>
        Dictionary<ServiceRegister, Dictionary<Type, object>> _instances_generic = new Dictionary<ServiceRegister, Dictionary<Type, object>>();

        public EasyInjector()
        {
            //內建 IServiceProvider IServiceScopeFactory

            // add IFactory 服務
#pragma warning disable 0618
            AddGenericService(SimpleLifetimes.Singleton, typeof(IFactory<>), (sp, genericArgs) =>
            {
                var implType = typeof(CFactory<>).MakeGenericType(genericArgs);
                var instance = implType.GetConstructor(new[] { typeof(IServiceProvider) }).Invoke(new object[] { sp });
                return instance;
            });
#pragma warning restore 0618

            // add IOptional 服務
            AddGenericService(SimpleLifetimes.Transient, typeof(IOptional<>), (sp, genericArgs) =>
            {
                var implType = typeof(OptionalService<>).MakeGenericType(genericArgs);
                var instance = implType.GetConstructor(new[] { typeof(IServiceProvider) }).Invoke(new object[] { sp });
                return instance;
            });
        }

        ~EasyInjector()
        {
            Dispose(false);
        }

        /// <summary>
        /// 增加泛型服務例如 IFactory
        /// </summary>
        public EasyInjector AddGenericService(SimpleLifetimes lifetimes, Type serviceType, Func<IServiceProvider, Type[], object> createGenericFunc)
        {
            lock (_serviceRegisters_lock)
            {
                var register = new ServiceRegister();
                register.IsGeneric = true;
                register.ServiceTypes = new[] { serviceType };
                register.Lifetimes = lifetimes;
                register.CreateGenericFunc = createGenericFunc;
                _serviceRegisters[serviceType] = register;
            }
            return this;
        }

        /// <summary>
        /// 註冊服務
        /// </summary>
        public EasyInjector AddTypedService<TService>(SimpleLifetimes lifetimes, IEnumerable<Type> serviceTypes, Func<IServiceProvider, TService> createFunc)
        {
            lock (_serviceRegisters_lock)
            {
                var register = new ServiceRegister();
                register.IsGeneric = false;
                register.ServiceTypes = serviceTypes;
                register.Lifetimes = lifetimes;
                register.CreateFunc = (sp) => createFunc(sp);
                foreach (var srvType in serviceTypes)
                    _serviceRegisters[srvType] = register;
            }
            return this;
        }

        /// <summary>
        /// 註冊服務
        /// </summary>
        public EasyInjector AddService(SimpleLifetimes lifetimes, IEnumerable<Type> serviceTypes, Func<IServiceProvider, Object> createFunc)
        {
            lock (_serviceRegisters_lock)
            {
                var register = new ServiceRegister();
                register.IsGeneric = false;
                register.ServiceTypes = serviceTypes;
                register.Lifetimes = lifetimes;
                register.CreateFunc = (sp) => createFunc(sp);
                foreach (var srvType in serviceTypes)
                    _serviceRegisters[srvType] = register;
            }
            return this;
        }

        /// <summary>
        /// 匯入註冊
        /// </summary>
        public EasyInjector ImportServices(IEnumerable<ServiceRegister> registers)
        {
            foreach (var register in registers)
            {
                if (register.IsGeneric == false)                
                    AddService(register.Lifetimes, register.ServiceTypes, register.CreateFunc);
                else
                    AddGenericService(register.Lifetimes, register.ServiceTypes.First(), register.CreateGenericFunc);
            }
            return this;
        }

        /// <summary>
        /// 註冊單一實例服務
        /// </summary>        
        /// <param name="createFunc">建構服務實例的方法，其依賴服務必須由 IServiceProvider取得</param>        
        public EasyInjector AddSingleton<T>(Func<IServiceProvider, T> createFunc)
        {
            AddTypedService(SimpleLifetimes.Singleton, new Type[] { typeof(T) }, createFunc);
            return this;
        }

        /// <summary>
        /// 註冊單一實例服務
        /// </summary>        
        /// <param name="createFunc">建構服務實例的方法，其依賴服務必須由 IServiceProvider取得</param>        
        public EasyInjector AddSingleton<TBase, TService>(Func<IServiceProvider, TService> createFunc) where TService : TBase
        {
            AddTypedService(SimpleLifetimes.Singleton, new Type[] { typeof(TBase), typeof(TService) }, createFunc);
            return this;
        }


        /// <summary>
        /// 註冊每次都生成一個的服務
        /// </summary>        
        /// <param name="createFunc">建構服務實例的方法，其依賴服務必須由 IServiceProvider取得</param>        
        public EasyInjector AddTransient<T>(Func<IServiceProvider, T> createFunc)
        {
            AddTypedService(SimpleLifetimes.Transient, new Type[] { typeof(T) }, createFunc);
            return this;
        }

        /// <summary>
        /// 註冊每次都生成一個的服務
        /// </summary>        
        /// <param name="createFunc">建構服務實例的方法，其依賴服務必須由 IServiceProvider取得</param>      
        public EasyInjector AddTransient<TBase, TService>(Func<IServiceProvider, TService> createFunc) where TService : TBase
        {
            AddTypedService(SimpleLifetimes.Transient, new Type[] { typeof(TBase), typeof(TService) }, createFunc);
            return this;
        }

        /// <summary>
        /// 註冊ScopeService
        /// </summary>        
        /// <param name="createFunc">建構服務實例的方法，其依賴服務必須由 IServiceProvider取得</param>        
        public EasyInjector AddScoped<T>(Func<IServiceProvider, T> createFunc)
        {
            AddTypedService(SimpleLifetimes.Scoped, new Type[] { typeof(T) }, createFunc);
            return this;
        }

        /// <summary>
        /// 註冊ScopeService
        /// </summary>        
        /// <param name="createFunc">建構服務實例的方法，其依賴服務必須由 IServiceProvider取得</param>        
        public EasyInjector AddScoped<TBase, TService>(Func<IServiceProvider, TService> createFunc) where TService : TBase
        {
            AddTypedService(SimpleLifetimes.Scoped, new Type[] { typeof(TBase), typeof(TService) }, createFunc);
            return this;
        }

        /// <summary>
        /// 取得服務(服務必須先註冊)
        /// </summary>
        /// <param name="serviceType">服務類型</param>
        /// <returns>服務</returns>
        public object GetService(Type serviceType)
        {
            return GetService(serviceType, null);
        }

        /// <summary>
        /// 取得服務(服務必須先註冊) 可以建立Scope類型的服務
        /// </summary>
        internal object GetService(Type serviceType, CScope scope)
        {
            //免註冊的預設服務
            if (serviceType == typeof(IServiceProvider))
                return scope == null ? this as IServiceProvider : scope;

            if (serviceType == typeof(IServiceScopeFactory))
                return this;

            lock (_serviceRegisters_lock)
            {
                ServiceRegister register;

                // 註冊項目找出
                if (!_serviceRegisters.TryGetValue(serviceType, out register))
                {

                    // 沒找到的話，看有沒有泛型註冊
                    if (serviceType.IsGenericType)
                    {
                        var genericType = serviceType.GetGenericTypeDefinition();
                        if (!_serviceRegisters.TryGetValue(genericType, out register))
                            return null;
                    }
                    else
                        return null;
                }

                // 取得服務 singleton
                if (register.Lifetimes == SimpleLifetimes.Singleton)
                {
                    // 一般服務
                    if (register.IsGeneric == false)
                    {
                        object instance;
                        if (_instances.TryGetValue(register, out instance) == false)
                        {
                            instance = register.CreateFunc(this);
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
                            instance = register.CreateGenericFunc(this, genericArgs);
                            instances.Add(serviceType, instance);
                        }
                        return instance;
                    }
                }
                // 取得服務 Scope 如果是Scope類型，必須有Scope傳入否則無法建立
                else if (register.Lifetimes == SimpleLifetimes.Scoped)
                {
                    if (scope == null)
                        throw new ApplicationException(string.Format("Service {0} is Scoped, you need create a scope first. please inject IServiceScopeFactory then use CreateScope().", serviceType.FullName));

                    // 一般服務
                    if (register.IsGeneric == false)
                    {                        
                        object instance;
                        //scope內已經產生直接回傳
                        if (scope._instances.TryGetValue(register, out instance))
                            return instance;
                        //新增一實例並放置Scope內
                        instance = register.CreateFunc(scope);
                        scope._instances.Add(register, instance);
                        return instance;
                    }
                    // 泛型服務
                    else
                    {
                        //找出自己專屬的Instancss
                        Dictionary<Type, object> instances;
                        object instance;
                        if (scope._instances_generic.TryGetValue(register, out instances) == false)
                        {
                            instances = new Dictionary<Type, object>();
                            scope._instances_generic.Add(register, instances);
                        }

                        // 不存在就新增一個
                        if (instances.TryGetValue(serviceType, out instance) == false)
                        {
                            var genericArgs = serviceType.GetGenericArguments();
                            instance = register.CreateGenericFunc(scope, genericArgs);
                            instances.Add(serviceType, instance);
                        }
                        return instance;
                    }

                }
                // 取得服務  Transient 
                else if (register.Lifetimes == SimpleLifetimes.Transient)
                {
                    // 一般服務
                    if (register.IsGeneric == false)
                    {
                        if (scope == null)
                            return register.CreateFunc(this);
                        else
                            return register.CreateFunc(scope);
                    }
                    // 泛型服務
                    else
                    {
                        var genericArgs = serviceType.GetGenericArguments();
                        if (scope == null)
                            return register.CreateGenericFunc(this, genericArgs);
                        else
                            return register.CreateGenericFunc(scope, genericArgs);
                    }
                }

                return null;
            }
        }

        public IServiceScope CreateScope()
        {
            return new CScope(this);
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
                lock (_serviceRegisters_lock)
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


                    _serviceRegisters.Clear();
                }
            }
            //不正常Dispose只要確保自身資源釋放即可
            else
            {
            }
            disposed = true;
        }

        IEnumerable<ServiceRegister> GetEnumerator1()
        {
            foreach (var i in _serviceRegisters)
                yield return i.Value;
        }

        IEnumerator<ServiceRegister> IEnumerable<ServiceRegister>.GetEnumerator()
        {
            return GetEnumerator1().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator1().GetEnumerator();
        }
    }
}


