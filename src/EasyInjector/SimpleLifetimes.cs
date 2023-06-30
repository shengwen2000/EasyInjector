using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyInjectors
{
#pragma warning disable 1591

    /// <summary>
    /// 服務存活範圍 同 .net core 定義
    /// </summary>
    public enum SimpleLifetimes
    {
        Singleton,
        Scoped,
        Transient
    }
}
