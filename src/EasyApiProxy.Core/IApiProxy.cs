using System.Collections;

namespace EasyApiProxys
{
    /// <summary>
    /// API Proxy 代理類別
    /// </summary>
    public interface IApiProxy<TAPI> where TAPI : class
    {
        /// <summary>
        /// 實例選項
        /// </summary>
        Hashtable Items { get; set; }

        /// <summary>
        /// API Object
        /// </summary>
        TAPI Object { get; set; }
    }


    /// <summary>
    /// API Proxy 代理類別
    /// </summary>
    public class ApiProxy<TAPI> : IApiProxy<TAPI>
        where TAPI : class
    {
        /// <summary>
        /// 實例選項
        /// </summary>
        public Hashtable Items { get; set; } = default!;

        /// <summary>
        /// API Interface
        /// </summary>
        public TAPI Object { get; set; } = default!;
    }
}
