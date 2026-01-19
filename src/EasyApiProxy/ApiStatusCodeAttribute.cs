using System;

namespace EasyApiProxys
{
    /// <summary>
    /// 指定 API 異常對應的 Http 狀態碼
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ApiStatusCodeAttribute : Attribute
    {
        /// <summary>
        /// 指定 Http 狀態碼
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 指定 API 異常對應的 Http 狀態碼
        /// </summary>
        /// <param name="statusCode">Http 狀態碼</param>
        public ApiStatusCodeAttribute(int statusCode)
        {
            StatusCode = statusCode;
        }
    }
}
