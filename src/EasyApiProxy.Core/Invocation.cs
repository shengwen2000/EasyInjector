using System.Reflection;

namespace EasyApiProxys
{
    /// <summary>
    /// Proxy類別被呼交的方法與參數
    /// </summary>
    /// <param name="Method">呼叫方法</param>
    /// <param name="Arguments">呼叫參數</param>
    /// <returns></returns>
    public record Invocation(MethodInfo Method, object?[] Arguments);
}

#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endif
