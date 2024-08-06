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
        /// 逾時秒數 (預設0代表使用整體預設值)
        /// </summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// 指定MethodName(default null 代表使用MethodName)
        /// </summary>
        public string Name { get; set; }        
    }
}
