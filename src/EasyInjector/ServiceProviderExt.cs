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
    }
}
