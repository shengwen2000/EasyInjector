using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyInjectors.Dev
{
    /// <summary>
    /// 標示複寫此方法，要複寫的方法必須標示才有作用。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class OverrideAttribute : Attribute
    {
    }
}
