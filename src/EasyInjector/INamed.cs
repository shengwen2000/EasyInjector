using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyInjectors
{
#pragma warning disable 0618,1591

    /// <summary>
    /// (Singleton|Scoped|Transient) 
    /// 1. 依據名稱來取得服務。通常用於一個服務類型卻有多個實例，利用名稱來取的想要的實例
    ///  例如 FTP管理服務: FTP(A)管理服務 FTP(B)管理服務
    /// 2. 如果服務有IDispose會自動釋放當此服務銷毀時
    /// </summary>
    public interface INamed<TService> where TService : class
    {
        /// <summary>
        /// 依據名稱來取得服務實例
        /// </summary>
        TService GetByName(string name);
    }

    /// <summary>
    /// (Singleton|Scoped) 提供當依據名稱如何來取得服務實例。應該於建構式中提供建立方法。
    /// 如果名稱對映的實例已經建立 再次以該名稱取得回傳原本取得的。
    /// </summary>
    [Obsolete("請改用NamedServiceV2 或改用 AddNamedSignleton 等")]
    public class NamedService<TService> : INamed<TService>, IDisposable where TService : class
    {
        Dictionary<string, TService> _ctxs = new Dictionary<string, TService>();

        private bool disposed = false;
        private Func<string, TService> _createFunc;
        object _lock_all = new object();


        /// <summary>建構方法</summary>
        /// <param name="createFunc">建立此服務的方法</param>
        public NamedService(Func<string, TService> createFunc)
        {
            _createFunc = createFunc;
        }

        /// <summary>解構方法</summary>
        ~NamedService()
        {
            Dispose(false);
        }

        /// <summary>依據名稱來取得服務實例</summary>
        public TService GetByName(string name)
        {
            TService ctx;
            if (_ctxs.TryGetValue(name, out ctx))
            {
                return ctx;
            }
            else
            {
                lock (_lock_all)
                {
                    if (_ctxs.TryGetValue(name, out ctx))
                    {
                        return ctx;
                    }
                    ctx = _createFunc(name);
                    _ctxs.Add(name, ctx);
                    return ctx;
                }
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            disposed = true;
            //正常Dispose，所有子項目一併施放
            if (disposing)
            {
                foreach (var x in _ctxs)
                {
                    if (x.Value != null)
                    {
                        var x1 = x.Value as IDisposable;
                        if (x1 != null)
                        {
                            try { x1.Dispose(); }
                            catch { }
                        }
                    }
                }
                _ctxs.Clear();
            }
            //不正常Dispose只要確保自身資源釋放即可
            else
            {
            }
        }
    }

    /// <summary>
    /// (Singleton|Scoped) 提供當依據名稱如何來取得服務實例。應該於建構式中提供建立方法。
    /// 如果名稱對映的實例已經建立 再次以該名稱取得回傳原本取得的。
    /// </summary>    
    public class NamedServiceV2<TService> : INamed<TService>, IDisposable where TService : class
    {
        Dictionary<string, TService> _ctxs = new Dictionary<string, TService>();

        private bool disposed = false;
        private readonly Func<IServiceProvider, string, TService> _createFunc;
        object _lock_all = new object();
        private readonly IServiceProvider _provider;


        /// <summary>建構方法</summary>
        /// <param name="createFunc">建立此服務的方法</param>
        /// <param name="provider">提供者</param>
        public NamedServiceV2(IServiceProvider provider, Func<IServiceProvider, string, TService> createFunc)
        {
            _provider = provider;
            _createFunc = createFunc;            
        }

        /// <summary>解構方法</summary>
        ~NamedServiceV2()
        {
            Dispose(false);
        }

        /// <summary>依據名稱來取得服務實例</summary>
        public TService GetByName(string name)
        {
            TService ctx;
            if (_ctxs.TryGetValue(name, out ctx))
            {
                return ctx;
            }
            else
            {
                lock (_lock_all)
                {
                    if (_ctxs.TryGetValue(name, out ctx))
                    {
                        return ctx;
                    }
                    ctx = _createFunc(_provider, name);
                    _ctxs.Add(name, ctx);
                    return ctx;
                }
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            disposed = true;
            //正常Dispose，所有子項目一併施放
            if (disposing)
            {
                foreach (var x in _ctxs)
                {
                    if (x.Value != null)
                    {
                        var x1 = x.Value as IDisposable;
                        if (x1 != null)
                        {
                            try { x1.Dispose(); }
                            catch { }
                        }
                    }
                }
                _ctxs.Clear();
            }
            //不正常Dispose只要確保自身資源釋放即可
            else
            {
            }
        }
    }

    /// <summary>
    /// (Transient) 提供當依據名稱如何來取得服務實例。應該於建構式中提供建立方法。
    /// 如果名稱對映的實例已經建立 再次以該名稱取得回傳原本取得的。
    /// </summary>
    /// <remarks>建構方法</remarks>
    
    public class NamedTransientService<TService> : INamed<TService> where TService : class
    {
        private readonly Func<IServiceProvider, string, TService> _createFunc;
        private readonly IServiceProvider _provider;

        /// <summary>建構方法</summary>
        /// <param name="createFunc">建立此服務的方法</param>
        /// <param name="provider">ServiceProvider</param>
        public NamedTransientService(
            IServiceProvider provider,
            Func<IServiceProvider, string, TService> createFunc)
        {
            _provider = provider;
            _createFunc = createFunc;
        }

        /// <summary>依據名稱來取得服務實例</summary>
        public TService GetByName(string name)
        {
            return _createFunc(_provider, name);
        }
    }

}
