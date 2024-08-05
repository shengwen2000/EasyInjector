using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyApiProxys
{
    /// <summary>
    /// Api Method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ApiMethodAttribute : Attribute
    {
        /// <summary>
        /// 指定MethodName(default null 代表使用MethodName)
        /// </summary>
        public string Name { get; set; }

        public ApiMethodAttribute()
        {           
        }
    }
}
