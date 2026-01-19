using System;

namespace EasyApiProxys
{
    /// <summary>
    /// 指定 API 異常對應的 Http 狀態碼
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ApiStatusCodeAttribute(int statusCode) : Attribute
    {
        public int StatusCode { get; } = statusCode;
    }
}
