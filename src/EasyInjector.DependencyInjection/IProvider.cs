
using System;
using Microsoft.Extensions.DependencyInjection;

namespace EasyInjectors
{
    /// <summary>
    /// (Transient)
    /// 特定服務的提供者。也可看成是弱相依。
    /// 如果服務為 Singleton 則取的唯一的那一個
    /// 如果服務為 Transient 每次都取的新一個
    /// 如果服務為 Scope 取得必須提供 Scope
    /// </summary>
    public interface IProvider<TService>
    {
        /// <summary>
        /// 由指定Provider取得服務
        /// </summary>
        TService? Get(IServiceScope scope);

        /// <summary>
        /// 由指定Scope取得服務
        /// (服務必須要有註冊否則異常)
        /// </summary>
        TService GetRequired(IServiceScope scope);

        /// <summary>
        /// 由預設Provider取得服務 所謂預設Provider就是取得此Optional的Provider
        /// </summary>
        TService? Get();

        /// <summary>
        /// 由預設Provider取得服務 所謂預設Provider就是取得此Optional的Provider
        /// (服務必須要有註冊要有否則異常)
        /// </summary>
        TService GetRequired();
    }

    class ProviderService<TService>(IServiceProvider defaultProvider) : IProvider<TService> where TService : class
    {
        public TService? Get(IServiceScope scope)
        {
            return scope.ServiceProvider.GetService<TService>();
        }

        public TService GetRequired(IServiceScope scope)
        {
            return scope.ServiceProvider.GetRequiredService<TService>();
        }

        public TService? Get()
        {
            return defaultProvider.GetService<TService>();
        }

        public TService GetRequired()
        {
            return defaultProvider.GetRequiredService<TService>();
        }
    }
}
