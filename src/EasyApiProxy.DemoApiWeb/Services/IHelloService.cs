using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmuApps.Services
{
    public interface IHelloService
    {
        string SayHello();
    }

    public class HelloService : IHelloService
    {
        public string SayHello()
        {
            return "Hello World";
        }
    }
}
