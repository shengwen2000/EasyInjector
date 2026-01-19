using System;

namespace EasyApiProxys
{
    /// <summary>
    /// 忽略 API 回應封裝處理
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class IgnoreApiResultAttribute : Attribute { }
}
