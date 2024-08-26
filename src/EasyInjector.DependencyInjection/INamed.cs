using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyInjectors
{
//#pragma warning disable 0618,1591

    /// <summary>
    /// (Singleton|Scoped|Transient)
    /// 1. 依據名稱來取得服務。通常用於一個服務類型卻有多個實例，利用名稱來取的想要的實例
    ///  例如 FTP管理服務: FTP(A)管理服務 FTP(B)管理服務
    /// 2. 如果服務有IDispose會自動釋放當此服務銷毀時
    /// </summary>
    public interface INamed<TService> where TService:class
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
    /// <remarks>建構方法</remarks>
    /// <param name="createFunc">建立此服務的方法</param>
    public class NamedService<TService>(
        IServiceProvider provider,
        Func<IServiceProvider, string, TService> createFunc) : INamed<TService>, IDisposable where TService : class
    {

        readonly Dictionary<string, TService> _ctxs = [];

        private bool disposed = false;
        readonly object _lock_all = new();

        /// <summary>解構方法</summary>
        ~NamedService()
        {
            Dispose(false);
        }

        /// <summary>依據名稱來取得服務實例</summary>
        public TService GetByName(string name)
        {
            if (_ctxs.TryGetValue(name, out TService? ctx))
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
                    ctx = createFunc(provider, name);
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
                        if (x.Value is IDisposable x1)
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
    /// <param name="createFunc">建立此服務的方法</param>
    public class NamedTransientService<TService>(
        IServiceProvider provider,
        Func<IServiceProvider, string, TService> createFunc) : INamed<TService> where TService : class
    {
        /// <summary>依據名稱來取得服務實例</summary>
        public TService GetByName(string name)
        {
            return createFunc(provider, name);
        }
    }
}
