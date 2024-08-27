using EasyInjectors;
using EasyInjectors.Dev;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 擴充方法
    /// </summary>
    public static class EasyInjectorExtension
    {
        /// <summary>
        /// 增加EasyInjector 特製的服務
        /// - IProvider 可以取得某服務
        /// - IServiceScope 可以注入當前的ServiceScope
        /// </summary>
        public static IServiceCollection AddEasyInjector(this IServiceCollection services) {

            services.AddTransient(typeof(IProvider<>), typeof(ProviderService<>));
            services.AddScoped<IServiceScope, ServiceScopeImpl>();

            return services;
        }

        /// <summary>
        /// 建立實例(類型不需要註冊) 自動填入建構子中的依賴服務
        /// </summary>
        public static object CreateInstance(this IServiceProvider provider, Type srvType)
        {
            var ctor1 = srvType.GetConstructors()
                .Where(x => x.IsPublic)
                .FirstOrDefault() ?? throw new ApplicationException(string.Format("類別{0}沒有公開建構子，無法生成實例", srvType.FullName));

            var pp = ctor1.GetParameters();
            if (pp.Length == 0)
            {
                var inst = Activator.CreateInstance(srvType) ?? throw new ApplicationException(string.Format("類別{0} 無法生成實例", srvType.FullName));
                return inst;
            }

            var vv = new object[pp.Length];

            for (var i = 0; i < pp.Length; i++)
            {
                var p1 = pp[i];
                var srv1 = provider.GetService(p1.ParameterType) ?? throw new ApplicationException(string.Format("類別{0} 要求注入服務{1} 失敗", srvType.FullName, p1.ParameterType.FullName));
                vv[i] = srv1;
            }
            {
                var inst = ctor1.Invoke(vv) ?? throw new ApplicationException(string.Format("類別{0} 無法生成實例", srvType.FullName));
                return inst;
            }
        }

        /// <summary>
        /// 建立實例(類型不需要註冊) 自動填入建構子中的依賴服務
        /// </summary>
        public static TService CreateInstance<TService>(this IServiceProvider provider)
            where TService : class
        {
            var inst = CreateInstance(provider, typeof(TService)) as TService
                ?? throw new ApplicationException(string.Format("類別{0} 無法生成實例", typeof(TService).FullName));
            return inst;
        }

        /// <summary>
        /// 註冊有名稱的Singleton服務 INamed
        /// </summary>
        public static IServiceCollection AddNamedSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, string, TService> createFunc)
            where TService : class
        {
            services.AddSingleton<INamed<TService>>(sp =>
            {
                return new NamedServiceV2<TService>(sp, createFunc);
            });
            return services;
        }

        /// <summary>
        /// 註冊有名稱的Scoped服務 INamed
        /// </summary>
        public static IServiceCollection AddNamedScoped<TService>(this IServiceCollection services, Func<IServiceProvider, string, TService> createFunc)
            where TService : class
        {
            services.AddScoped<INamed<TService>>(sp =>
            {
                return new NamedServiceV2<TService>(sp, createFunc);
            });
            return services;
        }

        /// <summary>
        /// 註冊有名稱的Transient服務 INamed
        /// </summary>
        public static IServiceCollection AddNamedTransient<TService>(this IServiceCollection services, Func<IServiceProvider, string, TService> createFunc)
            where TService : class
        {
            services.AddTransient<INamed<TService>>(sp =>
            {
                return new NamedTransientService<TService>(sp, createFunc);
            });
            return services;
        }

        /// <summary>
        /// 增加複寫服務
        /// </summary>
        /// <typeparam name="TService">要複寫的服務</typeparam>
        /// <typeparam name="TOverride">要複寫類別 (方法須標示[Override]才會複蓋) </typeparam>
        /// <param name="services">ServiceCollection</param>
        /// <returns></returns>
        public static void AddOverride<TService, TOverride>(this IServiceCollection services) where TService : class
        {
            var baseDescriptor1 = services.Where(x => x.ServiceType == typeof(TService))
                .LastOrDefault() ?? throw new ApplicationException($"Service {typeof(TService).FullName} not registered, cant be override");

            if (baseDescriptor1.Lifetime == ServiceLifetime.Singleton)
            {
                services.AddSingleton<TService>(sp =>
                {
                    var instance = CreateOverrideInstance<TService, TOverride>(sp, baseDescriptor1);
                    return (TService)instance;
                });
            }
            else if (baseDescriptor1.Lifetime == ServiceLifetime.Scoped)
            {
                services.AddScoped<TService>(sp =>
                {
                    var instance = CreateOverrideInstance<TService, TOverride>(sp, baseDescriptor1);
                    return (TService)instance;
                });
            }
            else if (baseDescriptor1.Lifetime == ServiceLifetime.Transient)
            {
                services.AddTransient<TService>(sp =>
                {
                    var instance = CreateOverrideInstance<TService, TOverride>(sp, baseDescriptor1);
                    return (TService)instance;
                });
            }
            else
                throw new NotSupportedException($"Not Support ${baseDescriptor1.Lifetime}");
        }

        /// <summary>
        /// 直接建立複寫服務實例 (複寫類別不需要註冊)
        /// </summary>
        /// <typeparam name="TService">服務類別</typeparam>
        /// <typeparam name="TOverride">複寫類別</typeparam>
        /// <param name="provider">服務提供</param>
        /// <returns>複寫服務</returns>
        static public TService CreateOverrideInstance<TService, TOverride>(this IServiceProvider provider) {

            var dd = GetServiceDescriptors(provider);
            var serviceDescriptor = dd.Where(x => x.ServiceType == typeof(TService))
                .LastOrDefault() ?? throw new ApplicationException($"找不到服務 {typeof(TService).FullName}註冊紀錄");
            var inst = (TService) CreateOverrideInstance<TService, TOverride>(provider, serviceDescriptor)
                ?? throw new ApplicationException($"無法建立服務 {typeof(TService).FullName}");
            return inst;
        }

        /// <summary>
        /// 直接建立複寫服務實例 (複寫類別不需要註冊)
        /// </summary>
        /// <typeparam name="TService">服務類別</typeparam>
        /// <typeparam name="TOverride">複寫類別</typeparam>
        /// <param name="provider">服務提供</param>
        /// <param name="serviceInstance">服務實例</param>
        /// <returns>複寫服務</returns>
        static public TService CreateOverrideInstance<TService, TOverride>(this IServiceProvider provider, TService serviceInstance)
            where TService : class {

            var inst = (TService) CreateOverrideInstance2<TService, TOverride>(provider, serviceInstance)
                ?? throw new ApplicationException($"無法建立服務 {typeof(TService).FullName}");
            return inst;
        }

        static IList<ServiceDescriptor> GetServiceDescriptors(IServiceProvider provider)
        {
            if (provider is ServiceProvider sp1)
                return GetServiceDescriptors(sp1);

            // 可能是Scope
            else
            {
                var prop1 = provider.GetType().GetProperty("RootProvider", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new NotSupportedException();
                var sp2 = prop1.GetValue(provider) as ServiceProvider
                    ?? throw new NotSupportedException();

                return GetServiceDescriptors(sp2);
            }

            static IList<ServiceDescriptor> GetServiceDescriptors(ServiceProvider sp)
            {
                var f1 = typeof(ServiceProvider).GetProperty("CallSiteFactory", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new NotSupportedException();
                var v1 = f1.GetValue(sp) ?? throw new NotSupportedException();
                var f2 = v1.GetType().GetField("_descriptors", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new NotSupportedException();
                var v2 = f2.GetValue(v1);
                var ss2 = v2 as IList<ServiceDescriptor> ?? throw new NotSupportedException();
                return ss2;
            }
        }

        /// <summary>
        /// 建立複寫服務實例
        /// </summary>
        /// <typeparam name="TService">服務類別</typeparam>
        /// <typeparam name="TOverride">複寫類別</typeparam>
        /// <param name="provider">服務提供</param>
        /// <param name="baseServiceDescriptor">基礎服務註冊</param>
        /// <returns>複寫服務</returns>
        static object CreateOverrideInstance<TService, TOverride>(IServiceProvider provider, ServiceDescriptor baseServiceDescriptor)
        {
            var overrideType = typeof(TOverride);

            var ctor1 = overrideType.GetConstructors()
                .Where(x => x.IsPublic)
                .FirstOrDefault() ?? throw new ApplicationException(string.Format("類別{0}沒有公開建構子，無法生成實例", overrideType.FullName));

            // 建構參數
            var pp = ctor1.GetParameters();
            var vv = new object[pp.Length];

            object? baseInstance = null;

            // 逐一取的建構參數服務
            for (var i = 0; i < pp.Length; i++)
            {
                var p1 = pp[i];
                // 如果是服務自身的話 提供舊的版本
                object? srv1 = null;
                if (p1.ParameterType == typeof(TService))
                {
                    srv1 = CreateInstanceByDescriptor(provider, baseServiceDescriptor);
                    baseInstance = srv1;
                }
                else
                    srv1 = provider.GetService(p1.ParameterType);
                if (srv1 == null)
                    throw new ApplicationException(string.Format("類別{0} 要求注入服務{1} 失敗", overrideType.FullName, p1.ParameterType.FullName));
                vv[i] = srv1;
            }

            {
                // 複寫的服務實例
                var overInstance = ctor1.Invoke(vv) ?? throw new ApplicationException(string.Format("類別{0} 無法生成實例", overrideType.FullName));

                // 基礎的服務實例
                baseInstance ??= CreateInstanceByDescriptor(provider, baseServiceDescriptor)
                    ?? throw new ApplicationException(string.Format("類別{0}的複寫基礎服務 無法生成實例", overrideType.FullName));

                // 產生代理類別
                var proxy = DispatchProxy.Create<TService, OverrideInterceptor>()
                    ?? throw new ApplicationException($"無法產生代理類別 ${typeof(TService).FullName}");
                var inteceptor1 = proxy as OverrideInterceptor
                    ?? throw new ApplicationException($"無法取得代理類別 ${typeof(TService).FullName}");
                inteceptor1.BaseInstance = baseInstance;
                inteceptor1.OverrideInstance = overInstance;
                return proxy;
            }
        }

        /// <summary>
        /// 建立複寫服務實例
        /// </summary>
        /// <typeparam name="TService">服務類別</typeparam>
        /// <typeparam name="TOverride">複寫類別</typeparam>
        /// <param name="provider">服務提供</param>
        /// <param name="baseInstance">服務實例</param>
        /// <returns>複寫服務</returns>
        static object CreateOverrideInstance2<TService, TOverride>(IServiceProvider provider, TService baseInstance)
            where TService : class
        {
            var overrideType = typeof(TOverride);

            var ctor1 = overrideType.GetConstructors()
                .Where(x => x.IsPublic)
                .FirstOrDefault() ?? throw new ApplicationException(string.Format("類別{0}沒有公開建構子，無法生成實例", overrideType.FullName));

            // 建構參數
            var pp = ctor1.GetParameters();
            var vv = new object[pp.Length];

            // 逐一取的建構參數服務
            for (var i = 0; i < pp.Length; i++)
            {
                var p1 = pp[i];
                // 如果是服務自身的話 提供舊的版本
                object? srv1 = null;
                if (p1.ParameterType == typeof(TService))
                    srv1 = baseInstance;
                else
                    srv1 = provider.GetService(p1.ParameterType);
                if (srv1 == null)
                    throw new ApplicationException(string.Format("類別{0} 要求注入服務{1} 失敗", overrideType.FullName, p1.ParameterType.FullName));
                vv[i] = srv1;
            }

            {
                // 複寫的服務實例
                var overInstance = ctor1.Invoke(vv) ?? throw new ApplicationException(string.Format("類別{0} 無法生成實例", overrideType.FullName));

                // 產生代理類別
                var proxy = DispatchProxy.Create<TService, OverrideInterceptor>()
                    ?? throw new ApplicationException($"無法產生代理類別 ${typeof(TService).FullName}");
                var inteceptor1 = proxy as OverrideInterceptor
                    ?? throw new ApplicationException($"無法取得代理類別 ${typeof(TService).FullName}");
                inteceptor1.BaseInstance = baseInstance;
                inteceptor1.OverrideInstance = overInstance;
                return proxy;
            }
        }

        /// <summary>
        /// 由 serviceDescriptor 來取得實例
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="serviceDescriptor"></param>
        /// <returns></returns>
        static object? CreateInstanceByDescriptor(IServiceProvider provider, ServiceDescriptor serviceDescriptor)
        {
            if (serviceDescriptor.ImplementationType != null)
            {
                var srvType = serviceDescriptor.ImplementationType;
                var ctor1 = srvType.GetConstructors()
                   .Where(x => x.IsPublic)
                   .FirstOrDefault() ?? throw new ApplicationException(string.Format("類別{0}沒有公開建構子，無法生成實例", srvType.FullName));

                var pp = ctor1.GetParameters();
                if (pp.Length == 0)
                {
                    var inst = Activator.CreateInstance(srvType) ?? throw new ApplicationException(string.Format("類別{0} 無法生成實例", srvType.FullName));
                    return inst;
                }

                var vv = new object[pp.Length];

                for (var i = 0; i < pp.Length; i++)
                {
                    var p1 = pp[i];
                    var srv1 = provider.GetService(p1.ParameterType) ?? throw new ApplicationException(string.Format("類別{0} 要求注入服務{1} 失敗", srvType.FullName, p1.ParameterType.FullName));
                    vv[i] = srv1;
                }
                {
                    var inst = ctor1.Invoke(vv) ?? throw new ApplicationException(string.Format("類別{0} 無法生成實例", srvType.FullName));
                    return inst;
                }
            }
            else if (serviceDescriptor.ImplementationInstance != null)
                return serviceDescriptor.ImplementationInstance;
            else if (serviceDescriptor.ImplementationFactory != null)
                return serviceDescriptor.ImplementationFactory.Invoke(provider);
            return null;
        }
    }
}
