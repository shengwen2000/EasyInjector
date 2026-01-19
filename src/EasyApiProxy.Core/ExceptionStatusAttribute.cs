using System;

namespace EasyApiProxys
{
    /// <summary>
    /// 指定特定 Exception 類型對應的 Http 狀態碼 (泛型版本，僅支援 .NET Core 6.0+)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class ExceptionStatusAttribute<TException>(int statusCode) : Attribute
        where TException : Exception
    {
        public Type ExceptionType => typeof(TException);
        public int StatusCode { get; } = statusCode;
    }
}
