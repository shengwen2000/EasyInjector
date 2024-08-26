using System.Reflection;

namespace EasyInjectors.Dev
{
    /// <summary>
    /// 複寫實作
    /// </summary>
    internal class OverrideInterceptor : DispatchProxy, IDisposable
    {
        public object OverrideInstance { get; set; } = default!;

        public object BaseInstance { get; set; } = default!;

        private bool disposed = false;

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
                if (BaseInstance != null && BaseInstance is IDisposable p1)
                {
                    p1.Dispose();
                }

                if (OverrideInstance != null && OverrideInstance is IDisposable p2)
                {
                    p2.Dispose();
                }
            }
        }

        /// <summary>
        /// 被呼叫的方法與參數
        /// </summary>
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == null) return null;
            if (args == null) return null;

            var parametersTypes = targetMethod.GetParameters().Select(y => y.ParameterType).ToArray();
            var m1 = OverrideInstance.GetType().GetMethod(targetMethod.Name, parametersTypes);

            if (m1 != null && m1.GetCustomAttributes(typeof(OverrideAttribute), false).Any())
                return m1.Invoke(OverrideInstance, args);
            else
            {
                var m2 = BaseInstance.GetType().GetMethod(targetMethod.Name, parametersTypes)
                    ?? throw new ApplicationException($"找不到方法 {targetMethod.Name}");
                return m2.Invoke(BaseInstance, args);
            }
        }
    }
}
