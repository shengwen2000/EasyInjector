

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
        /// singleton服務實例存放的地方
        /// </summary>
        InstanceCaches _caches = new InstanceCaches();

        public EasyInjector()
        {
            
#pragma warning disable 0618
            // add IFactory 服務
            AddGenericService(SimpleLifetimes.Singleton, typeof(IFactory<>), typeof(CFactory<>));

            // add IOptional 服務
            AddGenericService(SimpleLifetimes.Transient, typeof(IOptional<>), typeof(OptionalService<>));
#pragma warning restore 0618

            // add IProvider 服務
            AddGenericService(SimpleLifetimes.Transient, typeof(IProvider<>), typeof(ProviderService<>));
        }

        ~EasyInjector()
        {
            Dispose(false);
        }

        /// <summary>
        /// 建立實例 by ImplementType 內部使用
        /// </summary>
        T CreateInstance<T>(IServiceProvider provider, Type implType)
        {
            var srv = provider.CreateInstance(implType);
            return (T)srv;
        }

        /// <summary>
        /// 增加泛型服務例如 ILogger IOptions 
        /// </summary>
        public EasyInjector AddGenericService(SimpleLifetimes lifetimes, Type serviceType, Func<IServiceProvider, Type[], object> createGenericFunc)
        {
            var register = new ServiceRegister();
            register.IsGeneric = true;
            register.ServiceTypes = new[] { serviceType };
            register.Lifetimes = lifetimes;
            register.CreateGenericFunc = createGenericFunc;
            AddServiceInternal(register);
            return this;
        }

        /// <summary>
        /// 增加泛型服務例如 ILogger IOptions 
        /// </summary>
        public EasyInjector AddGenericService(SimpleLifetimes lifetimes, Type serviceType, Type implType)
        {
            //if (serviceType.IsAssignableFrom(implType) == false)
            //    throw new ApplicationException(string.Format("{0} 不能符合 {1}", implType, serviceType));        

            var register = new ServiceRegister();
            register.IsGeneric = true;
            register.ServiceTypes = new[] { serviceType };
            register.Lifetimes = lifetimes;
            register.CreateGenericFunc = (sp, typs) =>
            {
                var type1 = implType.MakeGenericType(typs);
                var srv1 = sp.CreateInstance(type1);
                return srv1;
            };
            AddServiceInternal(register);
            return this;
        }

        /// <summary>
        /// 註冊服務
        /// </summary>
        public EasyInjector AddTypedService<TService>(SimpleLifetimes lifetimes, IEnumerable<Type> serviceTypes, Func<IServiceProvider, TService> createFunc)
        {
            var register = new ServiceRegister();
            register.IsGeneric = false;
            register.ServiceTypes = serviceTypes;
            register.Lifetimes = lifetimes;
            register.CreateFunc = (sp) => createFunc(sp);
            AddServiceInternal(register);
            return this;
        }

        /// <summary>
        /// 註冊服務
        /// </summary>
        public EasyInjector AddService(SimpleLifetimes lifetimes, IEnumerable<Type> serviceTypes, Func<IServiceProvider, Object> createFunc)
        {
            var register = new ServiceRegister();
            register.IsGeneric = false;
            register.ServiceTypes = serviceTypes;
            register.Lifetimes = lifetimes;
            register.CreateFunc = (sp) => createFunc(sp);
            AddServiceInternal(register);
            return this;
        }

        /// <summary>
        /// 內部統一註冊點
        /// </summary>
        internal protected virtual void AddServiceInternal(ServiceRegister register, bool isTry = false) {
            lock (_serviceRegisters_lock)
            {
                foreach (var srvType in register.ServiceTypes)
                {
                    // 不存在才註冊
                    if (isTry)
                    {
                        if (_serviceRegisters.ContainsKey(srvType) == false)
                            _serviceRegisters[srvType] = register;
                    }
                    // 直接覆蓋 會檢查Lifetime 必須一致
                    else
                    {
                        ServiceRegister previous = null;
                        var has = _serviceRegisters.TryGetValue(srvType, out previous);

                        if (has && previous.Lifetimes != register.Lifetimes)
                            throw new ApplicationException(string.Format("EasyInjctor 複寫服務{0}時 發現其Lifetime不一致 目前{1} != 複寫{2} ", 
                                srvType.FullName,
                                previous.Lifetimes, 
                                register.Lifetimes));

                        _serviceRegisters[srvType] = register;
                    }
                }
            }
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
        /// 註冊單一實例服務
        /// </summary>                
        public EasyInjector AddSingleton<TInterface, TImplement>() where TImplement : TInterface
        {
            AddService(SimpleLifetimes.Singleton, new Type[] { typeof(TInterface) }, sp => CreateInstance<TInterface>(sp, typeof(TImplement)));
            return this;
        }

        /// <summary>
        /// 註冊每次都生成一個的服務
        /// </summary>                
        public EasyInjector AddTransient<TInterface, TImplement>() where TImplement : TInterface
        {
            AddService(SimpleLifetimes.Transient, new Type[] { typeof(TInterface) }, sp => CreateInstance<TInterface>(sp, typeof(TImplement)));
            return this;
        }        

        /// <summary>
        /// 註冊ScopeService
        /// </summary>                
        public EasyInjector AddScoped<TInterface, TImplement>() where TImplement : TInterface
        {
            AddTypedService(SimpleLifetimes.Scoped, new Type[] { typeof(TInterface) }, sp => CreateInstance<TInterface>(sp, typeof(TImplement)));
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
        /// 嘗試增加泛型服務例如 IFactory (沒有註冊過才註冊)
        /// </summary>
        public EasyInjector TryAddGenericService(SimpleLifetimes lifetimes, Type serviceType, Func<IServiceProvider, Type[], object> createGenericFunc)
        {
            var register = new ServiceRegister();
            register.IsGeneric = true;
            register.ServiceTypes = new[] { serviceType };
            register.Lifetimes = lifetimes;
            register.CreateGenericFunc = createGenericFunc;
            AddServiceInternal(register, isTry: true);
            return this;
        }

        /// <summary>
        /// 嘗試註冊服務(沒有註冊過才註冊)
        /// </summary>
        public EasyInjector TryAddTypedService<TService>(SimpleLifetimes lifetimes, IEnumerable<Type> serviceTypes, Func<IServiceProvider, TService> createFunc)
        {
            var register = new ServiceRegister();
            register.IsGeneric = false;
            register.ServiceTypes = serviceTypes;
            register.Lifetimes = lifetimes;
            register.CreateFunc = (sp) => createFunc(sp);
            AddServiceInternal(register, isTry: true);
            return this;
        }

        /// <summary>
        /// 嘗試註冊服務(沒有註冊過才註冊)
        /// </summary>
        public EasyInjector TryAddService(SimpleLifetimes lifetimes, IEnumerable<Type> serviceTypes, Func<IServiceProvider, Object> createFunc)
        {
            var register = new ServiceRegister();
            register.IsGeneric = false;
            register.ServiceTypes = serviceTypes;
            register.Lifetimes = lifetimes;
            register.CreateFunc = (sp) => createFunc(sp);
            AddServiceInternal(register, isTry: true);
            return this;
        }

        /// <summary>
        /// 註冊單一實例服務
        /// </summary>                
        public EasyInjector AddSingleton(Type srvType, Type implType)
        {
            if (!srvType.IsAssignableFrom(implType))
                throw new ApplicationException(string.Format("{0} 不能符合 {1}", implType, srvType));           

            AddTypedService(SimpleLifetimes.Singleton, new Type[] { srvType }, sp => sp.CreateInstance(implType));
            return this;
        }
        /// <summary>
        /// 註冊每次都生成一個的服務
        /// </summary>                
        public EasyInjector AddTransient(Type srvType, Type implType)
        {
            if (!srvType.IsAssignableFrom(implType))
                throw new ApplicationException(string.Format("{0} 不能符合 {1}", implType, srvType));
            AddTypedService(SimpleLifetimes.Transient, new Type[] { srvType }, sp => sp.CreateInstance(implType));
            return this;
        }

        /// <summary>
        /// 註冊ScopeService
        /// </summary>                
        public EasyInjector AddScoped(Type srvType, Type implType)
        {
            if (!srvType.IsAssignableFrom(implType))
                throw new ApplicationException(string.Format("{0} 不能符合 {1}", implType, srvType));
            AddTypedService(SimpleLifetimes.Scoped, new Type[] { srvType }, sp => sp.CreateInstance(implType));
            return this;
        }

        /// <summary>
        /// 嘗試註冊單一實例服務 (沒有註冊過才註冊)
        /// </summary>                
        public EasyInjector TryhAddSingleton(Type srvType, Type implType)
        {
            if (!srvType.IsAssignableFrom(implType))
                throw new ApplicationException(string.Format("{0} 不能符合 {1}", implType, srvType));
            TryAddTypedService(SimpleLifetimes.Singleton, new Type[] { srvType }, sp => sp.CreateInstance(implType));
            return this;
        }
        /// <summary>
        /// 嘗試註冊每次都生成一個的服務 (沒有註冊過才註冊)
        /// </summary>                
        public EasyInjector TryAddTransient(Type srvType, Type implType)
        {
            if (!srvType.IsAssignableFrom(implType))
                throw new ApplicationException(string.Format("{0} 不能符合 {1}", implType, srvType));
            TryAddTypedService(SimpleLifetimes.Transient, new Type[] { srvType }, sp => sp.CreateInstance(implType));
            return this;
        }

        /// <summary>
        /// 嘗試註冊ScopeService (沒有註冊過才註冊)
        /// </summary>                
        public EasyInjector TryAddScoped(Type srvType, Type implType)
        {
            if (!srvType.IsAssignableFrom(implType))
                throw new ApplicationException(string.Format("{0} 不能符合 {1}", implType, srvType));
            TryAddTypedService(SimpleLifetimes.Scoped, new Type[] { srvType }, sp => sp.CreateInstance(implType));
            return this;
        }

        /// <summary>
        /// 嘗試匯入註冊 (沒有註冊過才註冊)
        /// </summary>
        public EasyInjector TryImportServices(IEnumerable<ServiceRegister> registers)
        {
            foreach (var register in registers)
            {
                if (register.IsGeneric == false)
                    TryAddService(register.Lifetimes, register.ServiceTypes, register.CreateFunc);
                else
                    TryAddGenericService(register.Lifetimes, register.ServiceTypes.First(), register.CreateGenericFunc);
            }
            return this;
        }


        /// <summary>
        /// 嘗試註冊單一實例服務 (沒有註冊過才註冊)
        /// </summary>        
        /// <param name="createFunc">建構服務實例的方法，其依賴服務必須由 IServiceProvider取得</param>        
        public EasyInjector TryAddSingleton<T>(Func<IServiceProvider, T> createFunc)
        {
            TryAddTypedService(SimpleLifetimes.Singleton, new Type[] { typeof(T) }, createFunc);
            return this;
        }

        /// <summary>
        /// 嘗試註冊單一實例服務 (沒有註冊過才註冊)
        /// </summary>        
        /// <param name="createFunc">建構服務實例的方法，其依賴服務必須由 IServiceProvider取得</param>        
        public EasyInjector TryAddSingleton<TBase, TService>(Func<IServiceProvider, TService> createFunc) where TService : TBase
        {
            TryAddTypedService(SimpleLifetimes.Singleton, new Type[] { typeof(TBase), typeof(TService) }, createFunc);
            return this;
        }

        /// <summary>
        /// 嘗試註冊每次都生成一個的服務 (沒有註冊過才註冊)
        /// </summary>        
        /// <param name="createFunc">建構服務實例的方法，其依賴服務必須由 IServiceProvider取得</param>        
        public EasyInjector TryAddTransient<T>(Func<IServiceProvider, T> createFunc)
        {
            TryAddTypedService(SimpleLifetimes.Transient, new Type[] { typeof(T) }, createFunc);
            return this;
        }

        /// <summary>
        /// 嘗試註冊每次都生成一個的服務 (沒有註冊過才註冊)
        /// </summary>        
        /// <param name="createFunc">建構服務實例的方法，其依賴服務必須由 IServiceProvider取得</param>      
        public EasyInjector TryAddTransient<TBase, TService>(Func<IServiceProvider, TService> createFunc) where TService : TBase
        {
            TryAddTypedService(SimpleLifetimes.Transient, new Type[] { typeof(TBase), typeof(TService) }, createFunc);
            return this;
        }

        /// <summary>
        /// 嘗試註冊ScopeService (沒有註冊過才註冊)
        /// </summary>        
        /// <param name="createFunc">建構服務實例的方法，其依賴服務必須由 IServiceProvider取得</param>        
        public EasyInjector TryAddScoped<T>(Func<IServiceProvider, T> createFunc)
        {
            TryAddTypedService(SimpleLifetimes.Scoped, new Type[] { typeof(T) }, createFunc);
            return this;
        }

        /// <summary>
        /// 嘗試註冊ScopeService (沒有註冊過才註冊)
        /// </summary>        
        /// <param name="createFunc">建構服務實例的方法，其依賴服務必須由 IServiceProvider取得</param>        
        public EasyInjector TryAddScoped<TBase, TService>(Func<IServiceProvider, TService> createFunc) where TService : TBase
        {
            TryAddTypedService(SimpleLifetimes.Scoped, new Type[] { typeof(TBase), typeof(TService) }, createFunc);
            return this;
        }


        /// <summary>
        /// 註冊單一實例服務
        /// </summary>                
        public EasyInjector TryAddSingleton<TInterface, TImplement>() where TImplement : TInterface
        {
            TryAddService(SimpleLifetimes.Singleton, new Type[] { typeof(TInterface) }, sp => CreateInstance<TInterface>(sp, typeof(TImplement)));
            return this;
        }

        /// <summary>
        /// 註冊每次都生成一個的服務
        /// </summary>                
        public EasyInjector TryAddTransient<TInterface, TImplement>() where TImplement : TInterface
        {
            TryAddService(SimpleLifetimes.Transient, new Type[] { typeof(TInterface) }, sp => CreateInstance<TInterface>(sp, typeof(TImplement)));
            return this;
        }

        /// <summary>
        /// 註冊ScopeService
        /// </summary>                
        public EasyInjector TryAddScoped<TInterface, TImplement>() where TImplement : TInterface
        {
            TryAddTypedService(SimpleLifetimes.Scoped, new Type[] { typeof(TInterface) }, sp => CreateInstance<TInterface>(sp, typeof(TImplement)));
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
        internal object GetService(Type serviceType, ServiceScope scope)
        {
            //免註冊的預設服務
            if (serviceType == typeof(IServiceProvider))
                return scope == null ? this as IServiceProvider : scope;

            if (serviceType == typeof(IServiceScopeFactory))
                return this;

            if (serviceType == typeof(IServiceScope))
                return scope;

            lock (_serviceRegisters_lock)
            {
                // 找出服務註冊項目
                var register = FindRegister(serviceType);
                if (register == null) return null;      

                // 取得服務 singleton
                if (register.Lifetimes == SimpleLifetimes.Singleton)
                {
                    var instance = _caches.GetOrCreateInstance(this, register, serviceType);
                    return instance;
                }
                // 取得服務 Scope 如果是Scope類型，必須有Scope傳入否則無法建立
                else if (register.Lifetimes == SimpleLifetimes.Scoped)
                {
                    if (scope == null)
                        throw new ApplicationException(string.Format("Service {0} is Scoped, you need create a scope first. please inject IServiceScopeFactory then use CreateScope().", serviceType.FullName));

                    var instance = scope._caches.GetOrCreateInstance(scope, register, serviceType);
                    return instance;
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

        /// <summary>
        /// 找出服務註冊項目
        /// </summary>
        /// <param name="serviceType">服務類型</param>
        public ServiceRegister FindRegister(Type serviceType)
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
            return register;
        }

        /// <summary>
        /// 找出服務註冊項目
        /// </summary>        
        public ServiceRegister FindRegister<TServiceType>()
        {
            return FindRegister(typeof(TServiceType));
        }

        public IServiceScope CreateScope()
        {
            return new ServiceScope(this);
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
                _caches.Dispose();
            }
            //不正常Dispose只要確保自身資源釋放即可
            else
            {
            }           
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


