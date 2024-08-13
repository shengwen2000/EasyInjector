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
    internal class OverrideInterceptor : IInterceptor
    {
        /// <summary>
        /// 複寫物件
        /// </summary>
        private readonly object _overrideone;

        /// <summary>
        /// 基礎物件
        /// </summary>
        private readonly object _baseone;

        public OverrideInterceptor(object overrideone, object baseone)
        {
            _overrideone = overrideone;
            _baseone = baseone;
        }

        public void Intercept(IInvocation invocation)
        {
            // 呼叫方法有 [Override] 呼叫複寫類別方法
            // 呼叫方法沒有 [Override] 呼叫基礎類別方法

            var parametersTypes = invocation.Method.GetParameters().Select(y => y.ParameterType).ToArray();
            var m1 = _overrideone.GetType().GetMethod(invocation.Method.Name, parametersTypes);
            var hasOverride = m1.GetCustomAttributes(typeof(OverrideAttribute), false).Any();
            if (hasOverride)
                invocation.ReturnValue = m1.Invoke(_overrideone, invocation.Arguments);
            else
            {
                var m2 = _baseone.GetType().GetMethod(invocation.Method.Name, parametersTypes);
                invocation.ReturnValue = m2.Invoke(_baseone, invocation.Arguments);
            }
        }
    }        
}
