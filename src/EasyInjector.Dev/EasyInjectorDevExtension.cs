using Castle.DynamicProxy;
using EasyInjectors.Dev;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyInjectors
{
    /// <summary>
    /// 擴充方法
    /// </summary>
    public static class EasyInjectorDevExtension
    {
        /// <summary>
        /// 增加複寫服務
        /// </summary> 
        /// <typeparam name="TService">要複寫的服務</typeparam>
        /// <typeparam name="TOverride">要複寫類別 (方法須標示[Override]才會複蓋) </typeparam>
        /// <param name="injector">Injector</param>
        /// <returns></returns>
        public static void AddOverride<TService, TOverride>(this EasyInjector injector)            
        {
            // 前一個
            var r1 = injector.FindRegister<TService>();
            if (r1 == null)
                throw new NotSupportedException(string.Format("服務{0}沒有註冊", typeof(TService).FullName));

            if (r1.IsGeneric) 
                throw new NotSupportedException("此方法僅支援非泛型服務");

            injector.AddService(r1.Lifetimes, new[] { typeof(TService) },
                sp => CreateOverrideInstance<TService, TOverride>(sp, r1));
        }

        /// <summary>
        /// 建立複寫服務實例
        /// </summary>
        /// <typeparam name="TService">服務類別</typeparam>
        /// <typeparam name="TOverride">複寫類別</typeparam>
        /// <param name="provider">服務提供</param>
        /// <param name="baseServiceRegister">基礎服務註冊</param>
        /// <returns>複寫服務</returns>
        static object CreateOverrideInstance<TService, TOverride>(IServiceProvider provider, ServiceRegister baseServiceRegister)
        {
            var overrideType = typeof(TOverride);

            var ctor1 = overrideType.GetConstructors()
                .Where(x => x.IsPublic)
                .FirstOrDefault();

            if (ctor1 == null)
                throw new ApplicationException(string.Format("類別{0}沒有公開建構子，無法生成實例", overrideType.FullName));

            // 建構參數
            var pp = ctor1.GetParameters();
            var vv = new object[pp.Length];

            // 逐一取的建構參數服務
            for (var i = 0; i < pp.Length; i++)
            {
                var p1 = pp[i];
                // 如果是服務自身的話 提供舊的版本
                object srv1 = null;
                if (p1.ParameterType == typeof(TService))
                    srv1 = baseServiceRegister.CreateFunc.Invoke(provider);
                else
                    srv1 = provider.GetService(p1.ParameterType);
                if (srv1 == null)
                    throw new ApplicationException(string.Format("類別{0} 要求注入服務{1} 失敗", overrideType.FullName, p1.ParameterType.FullName));
                vv[i] = srv1;
            }

            {
                // 複寫的服務實例
                var overOne = ctor1.Invoke(vv);
                if (overOne == null)
                    throw new ApplicationException(string.Format("類別{0} 無法生成實例", overrideType.FullName));

                // 基礎的服務實例
                object baseOne = baseServiceRegister.CreateFunc.Invoke(provider);
                if (baseOne == null)
                    throw new ApplicationException(string.Format("類別{0}的複寫基礎服務 無法生成實例", overrideType.FullName));

                // 產生代理類別
                var generator = new ProxyGenerator();
                var proxy = generator.CreateInterfaceProxyWithoutTarget(typeof(TService), new OverrideInterceptor(overOne, baseOne));
                return proxy;
            }
        }

        /// <summary>
        /// 增加複寫服務(泛型)
        /// </summary>     
        /// <param name="injector">Injector</param>
        /// <param name="serviceType">要複寫的服務</param>
        /// <param name="overrideType">要複寫類別 (方法須標示[Override]才會複蓋)</param>
        /// <returns></returns>
        public static void AddOverrideGeneric(this EasyInjector injector, Type serviceType, Type overrideType)
        {
            // 前一個
            var r1 = injector.FindRegister(serviceType);
            if (r1 == null)
                throw new NotSupportedException(string.Format("服務{0}沒有註冊", serviceType.FullName));

            if (r1.IsGeneric==false)
                throw new NotSupportedException("此方法僅支援泛型服務");

            injector.AddGenericService(r1.Lifetimes, serviceType, (sp, targs) =>
                CreateOverrideGenericInstance(sp, r1, serviceType, overrideType, targs));
        }

        /// <summary>
        /// 建立複寫服務實例
        /// </summary>     
        /// <param name="provider">服務提供</param>
        /// <param name="baseServiceRegister">基礎服務註冊</param>
        /// <param name="typeArgs">泛型參數</param>
        /// <param name="serviceType">服務類別</param>
        /// <param name="overrideType">要覆蓋的類別</param>
        /// <returns>複寫服務</returns>
        static object CreateOverrideGenericInstance(IServiceProvider provider, ServiceRegister baseServiceRegister, Type serviceType, Type overrideType, Type[] typeArgs)
        {
            var overrideType1 = overrideType.MakeGenericType(typeArgs);
            var ctor1 = overrideType1.GetConstructors()
                .Where(x => x.IsPublic)
                .FirstOrDefault();

            if (ctor1 == null)
                throw new ApplicationException(string.Format("類別{0}沒有公開建構子，無法生成實例", overrideType1.FullName));

            var serviceType1 = serviceType.MakeGenericType(typeArgs);

            // 建構參數
            var pp = ctor1.GetParameters();
            var vv = new object[pp.Length];

            // 逐一取的建構參數服務
            for (var i = 0; i < pp.Length; i++)
            {
                var p1 = pp[i];
                // 如果是服務自身的話 提供舊的版本
                object srv1 = null;
                if (p1.ParameterType == serviceType1)
                    srv1 = baseServiceRegister.CreateGenericFunc(provider, typeArgs);
                else
                    srv1 = provider.GetService(p1.ParameterType);
                if (srv1 == null)
                    throw new ApplicationException(string.Format("類別{0} 要求注入服務{1} 失敗", overrideType.FullName, p1.ParameterType.FullName));
                vv[i] = srv1;
            }

            {
                // 複寫的服務實例
                var overOne = ctor1.Invoke(vv);
                if (overOne == null)
                    throw new ApplicationException(string.Format("類別{0} 無法生成實例", overrideType.FullName));

                // 基礎的服務實例
                object baseOne = baseServiceRegister.CreateGenericFunc.Invoke(provider, typeArgs);
                if (baseOne == null)
                    throw new ApplicationException(string.Format("類別{0}的複寫基礎服務 無法生成實例", overrideType.FullName));

                // 產生代理類別
                var generator = new ProxyGenerator();
                var proxy = generator.CreateInterfaceProxyWithoutTarget(serviceType1, new OverrideInterceptor(overOne, baseOne));
                return proxy;
            }
        }

    }
}
