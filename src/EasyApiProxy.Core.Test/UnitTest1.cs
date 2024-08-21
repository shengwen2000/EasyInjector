using System.Reflection;
using EasyApiProxys.DemoApis;

namespace EasyApiProxy.Core.Test;


public class UnitTest1
{
    [Test]
    public void TestMethod1()
    {
        var proxy = DispatchProxy.Create<IDemoApi, DemoProxy>();

        var info = proxy.GetServerInfo();
        Assert.AreEqual("Hello World", info);

        var p1 = proxy as DemoProxy;
        Assert.IsNotNull(p1);
        p1.Name = "123";

        Assert.AreEqual("123", proxy.GetServerInfo());
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