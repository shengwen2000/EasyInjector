using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyInjectors
{
    /// <summary>
    /// 服務註冊項目
    /// </summary>
    public class ServiceRegister
    {
        /// <summary>
        /// 是泛型服務?
        /// </summary>
        public bool IsGeneric { get; set; }

        /// <summary>
        /// 類型 Singleton | Scoped  | Transient
        /// </summary>
        public SimpleLifetimes Lifetimes { get; set; }

        /// <summary>
        /// 註冊的服務 可多個指向同一個服務 e.g. (IAuthService, IMobileAuthService) to MobileAuthService
        /// </summary>
        public IEnumerable<Type> ServiceTypes { get; set; }

        /// <summary>
        /// 建構一般服務實例的方法，其依賴服務必須由 IServiceProvider取得
        /// </summary>
        public Func<IServiceProvider, object> CreateFunc { get; set; }

        /// <summary>
        /// 建構泛型服務實例的方法，其依賴服務必須由 IServiceProvider取得
        /// </summary>
        public Func<IServiceProvider, Type[], object> CreateGenericFunc { get; set; }
       
    }
}
