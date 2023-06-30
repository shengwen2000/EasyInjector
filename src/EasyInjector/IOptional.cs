
using System;

namespace EasyInjectors
{
    /// <summary>
    /// (Transient) 選擇性取得的服務。於必要時才取得服務。
    /// </summary>   
    public interface IOptional<TService>
    {
        /// <summary>
        /// 由指定Provider取得服務
        /// </summary>
        TService Get(IServiceProvider provider);

        /// <summary>
        /// 由指定Provider取得服務
        /// (服務必須要有註冊否則異常)
        /// </summary>
        TService GetRequired(IServiceProvider provider);

        /// <summary>
        /// 由預設Provider取得服務 所謂預設Provider就是取得此Optional的Provider
        /// </summary>
        TService Get();

        /// <summary>
        /// 由預設Provider取得服務 所謂預設Provider就是取得此Optional的Provider
        /// (服務必須要有註冊要有否則異常)
        /// </summary>
        TService GetRequired();
    }

    class OptionalService<TService> : IOptional<TService> where TService : class
    {
        private IServiceProvider _defaultProvider;

        public OptionalService(IServiceProvider defaultProvider)
        {
            _defaultProvider = defaultProvider;
        }

        public TService Get(IServiceProvider provider)
        {
            return provider.GetService<TService>();
        }

        public TService GetRequired(IServiceProvider provider)
        {
            return provider.GetRequiredService<TService>();
        }

        public TService Get()
        {
            return _defaultProvider.GetService<TService>();
        }

        public TService GetRequired()
        {
            return _defaultProvider.GetRequiredService<TService>();
        }
    }
}
