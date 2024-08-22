using System.Reflection;
using EasyApiProxys.DemoApis;

namespace EasyApiProxy.Core.Test;


public class DispatchProxyTest
{
    [Test]
    public void DispatchTestMethod1()
    {
        var proxy = DispatchProxy.Create<IDemoApi, DemoProxy>();

        var info = proxy.GetServerInfo();
        Assert.That(info, Is.EqualTo("Hello World"));

        var p1 = proxy as DemoProxy;
        Assert.That(p1, Is.Not.Null);
        p1.Name = "123";

        Assert.That(proxy.GetServerInfo(), Is.EqualTo("123"));
    }

    public class DemoProxy : DispatchProxy
    {
        public string Name {get; set;} = "Hello World";

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name == "GetServerInfo")
                return Name;
            throw new NotImplementedException();
        }
    }
}