using EasyInjectors.Dev;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EasyInjectors
{
    /// <summary>
    /// 擴充方法
    /// </summary>
    public static class EasyInjectorExtension
    {
        /// <summary>
        /// 增加複寫服務
        /// </summary>
        /// <typeparam name="TService">要複寫的服務</typeparam>
        /// <typeparam name="TOverride">要複寫類別 (方法須標示[Override]才會複蓋) </typeparam>
        /// <param name="injector">Injector</param>
        /// <returns></returns>
        public static void AddOverride<TService, TOverride>(this ServiceCollection injector) where TService : class
        {
            var baseDescriptor1 = injector.Where(x => x.ServiceType == typeof(TService))
                .LastOrDefault() ?? throw new ApplicationException($"Service {typeof(TService).FullName} not registered, cant be override");

            if (baseDescriptor1.Lifetime == ServiceLifetime.Singleton)
            {
                injector.AddSingleton<TService>(sp =>
                {
                    var instance = CreateOverrideInstance<TService, TOverride>(sp, baseDescriptor1);
                    return (TService)instance;
                });
            }
            else if (baseDescriptor1.Lifetime == ServiceLifetime.Scoped)
            {
                injector.AddScoped<TService>(sp =>
                {
                    var instance = CreateOverrideInstance<TService, TOverride>(sp, baseDescriptor1);
                    return (TService)instance;
                });
            }
            else if (baseDescriptor1.Lifetime == ServiceLifetime.Transient)
            {
                injector.AddTransient<TService>(sp =>
                {
                    var instance = CreateOverrideInstance<TService, TOverride>(sp, baseDescriptor1);
                    return (TService)instance;
                });
            }
            else
                throw new NotSupportedException($"Not Support ${baseDescriptor1.Lifetime}");
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
                if (p1.ParameterType == typeof(TService)) {
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
