using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyInjectors
{
    /// <summary>
    /// 服務擴充方法
    /// </summary>
    public static class ServiceProviderExt
    {
        /// <summary>
        /// 取得選擇性服務 (可能會取得null)
        /// </summary>
        public static T GetService<T>(this IServiceProvider provider) where T : class
        {
            return provider.GetService(typeof(T)) as T;
        }

        /// <summary>
        /// 取得必要服務 (沒有服務的話會異常)
        /// </summary>
        public static T GetRequiredService<T>(this IServiceProvider provider) where T : class
        {
            var srv = provider.GetService(typeof(T)) as T;
            if (srv == null)
                throw new NotImplementedException(string.Format("Service {0} not registered", typeof(T).FullName));
            return srv;
        }

        /// <summary>
        /// 建立實例(類型不需要註冊) 自動填入建構子中的依賴服務
        /// </summary>
        public static object CreateInstance(this IServiceProvider provider, Type srvType)
        {
            var ctor1 = srvType.GetConstructors()
                .Where(x => x.IsPublic)
                .FirstOrDefault();

            if (ctor1 == null)
                throw new ApplicationException(string.Format("類別{0}沒有公開建構子，無法生成實例", srvType.FullName));

            var pp = ctor1.GetParameters();
            if (pp.Length == 0)
            {
                var inst = Activator.CreateInstance(srvType);
                if (inst == null)
                    throw new ApplicationException(string.Format("類別{0} 無法生成實例", srvType.FullName));
                return inst;
            }

            var vv = new object[pp.Length];

            for (var i = 0; i < pp.Length; i++)
            {
                var p1 = pp[i];
                var srv1 = provider.GetService(p1.ParameterType);
                if (srv1 == null)
                    throw new ApplicationException(string.Format("類別{0} 要求注入服務{1} 失敗", srvType.FullName, p1.ParameterType.FullName));
                vv[i] = srv1;
            }
            {
                var inst = Activator.CreateInstance(srvType, vv);
                if (inst == null)
                    throw new ApplicationException(string.Format("類別{0} 無法生成實例", srvType.FullName));
                return inst;
            }
        }

        /// <summary>
        /// 建立實例(類型不需要註冊) 自動填入建構子中的依賴服務
        /// </summary>
        public static TInstance CreateInstance<TInstance>(this IServiceProvider provider) where TInstance : class
        {
            var inst = CreateInstance(provider, typeof(TInstance)) as TInstance;
            return inst;
        }
    }
}
