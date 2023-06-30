using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyInjectors
{
    /// <summary>
    /// 請參考 .Net Core的定義
    /// </summary>
    public interface IServiceScopeFactory
    {
        /// <summary>
        /// 建立服務 Scope
        /// </summary>
        IServiceScope CreateScope();
    }
}
