using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyInjectors.Dev
{
    /// <summary>
    /// 複寫實作
    /// </summary>
    internal class OverrideInterceptor : IInterceptor, IDisposable
    {
        private bool disposed = false;

        /// <summary>
        /// 複寫物件
        /// </summary>
        private readonly object _overrideInstance;

        /// <summary>
        /// 基礎物件
        /// </summary>
        private readonly object _baseInstance;

        public OverrideInterceptor(object overrideone, object baseone)
        {
            _overrideInstance = overrideone;
            _baseInstance = baseone;
        }

        ~OverrideInterceptor()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            disposed = true;
            //正常Dispose，所有子項目一併施放
            if (disposing)
            {
                if (_baseInstance != null && _baseInstance is IDisposable)
                {
                    (_baseInstance as IDisposable).Dispose();
                }

                if (_overrideInstance != null && _overrideInstance is IDisposable)
                {
                    (_overrideInstance as IDisposable).Dispose();
                }
            }
        }

        public void Intercept(IInvocation invocation)
        {
            // 呼叫方法有 [Override] 呼叫複寫類別方法
            // 呼叫方法沒有 [Override] 呼叫基礎類別方法
            try
            {
                var parametersTypes = invocation.Method.GetParameters().Select(y => y.ParameterType).ToArray();
                var m1 = _overrideInstance.GetType().GetMethod(invocation.Method.Name, parametersTypes);
                var hasOverride = m1.GetCustomAttributes(typeof(OverrideAttribute), false).Any();
                if (hasOverride)
                    invocation.ReturnValue = m1.Invoke(_overrideInstance, invocation.Arguments);
                else
                {
                    var m2 = _baseInstance.GetType().GetMethod(invocation.Method.Name, parametersTypes);
                    invocation.ReturnValue = m2.Invoke(_baseInstance, invocation.Arguments);
                }
            }
            // 遇到執行方法異常 回拋 innerException
            catch (System.Reflection.TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                throw;
            }
        }
    }        
}
