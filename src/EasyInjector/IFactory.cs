using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyInjectors
{
#pragma warning disable 1591,0618

    [Obsolete("廢棄請改用 IProvider")]
    public interface IFactory<TService>
    {
        TService Create();

        TService Create(IServiceScope scope);
    }

    class CFactory<TService> : IFactory<TService> where TService : class
    {
        private IServiceProvider _serviceProvider;
        public CFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TService Create()
        {
            return _serviceProvider.GetService(typeof(TService)) as TService;
        }

        public TService Create(IServiceScope scope)
        {
            return scope.ServiceProvider.GetService(typeof(TService)) as TService;
        }
    }
}
