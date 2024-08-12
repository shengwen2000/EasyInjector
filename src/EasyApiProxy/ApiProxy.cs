using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyApiProxys
{
    /// <summary>
    /// API Proxy 代理類別
    /// </summary>
    public class ApiProxy<TAPI> where TAPI : class
    {
        /// <summary>
        /// 實例選項
        /// </summary>
        public Hashtable Items { get; set; }

        /// <summary>
        /// API Interface
        /// </summary>
        public TAPI Api { get; set; }
    }
}
