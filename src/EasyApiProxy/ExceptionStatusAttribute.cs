using System;

namespace EasyApiProxys
{
    /// <summary>
    /// 指定特定 Exception 類型對應的 Http 狀態碼
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class ExceptionStatusAttribute : Attribute
    {
        public Type ExceptionType { get; set; }
        public int StatusCode { get; set; }
        public ExceptionStatusAttribute(Type exceptionType, int statusCode)
        {
            ExceptionType = exceptionType;
            StatusCode = statusCode;
        }
    }
}
