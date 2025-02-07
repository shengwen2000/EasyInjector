using EasyApiProxys.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyApiProxys
{
    /// <summary>
    /// 提供實例呼叫 攔截
    /// - 設定 Header
    /// - RequestLast 呼叫前 所有 Pipe 最後一個
    /// - ResponeFirst 返回後 所有 Pipe 第一個
    /// </summary>
    public static class InstanceCallExtension
    {
        /// <summary>
        /// 套用 實例呼叫 攔截
        /// </summary>
        internal static ApiProxyBuilder UseInstanceCallHandler(ApiProxyBuilder builder)
        {
            var handler = new InstanceHandler(builder.Options.Step1, builder.Options.Step2, builder.Options.Step3, builder.Options.Step4);
            builder.Options.Handlers.Add(handler);

            builder.Options.Step1 = handler.Step1;
            builder.Options.Step2 = handler.Step2;
            builder.Options.Step3 = handler.Step3;
            builder.Options.Step4 = handler.Step4;
            return builder;
        }
    }

    internal class InstanceHandler
    {
        readonly Func<StepContext, Task> _step1;
        readonly Func<StepContext, Task> _step2;
        readonly Func<StepContext, Task> _step3;
        readonly Func<StepContext, Task> _step4;

        public InstanceHandler(Func<StepContext, Task> func1, Func<StepContext, Task> func2, Func<StepContext, Task> func3, Func<StepContext, Task> func4)
        {
            _step1 = func1;
            _step2 = func2;
            _step3 = func3;
            _step4 = func4;
        }

        public async Task Step1(StepContext context)
        {
            if (_step1 != null)
                await this._step1(context);
            var action1 = context.InstanceItems["InstanceCall_Step1"] as Func<StepContext, Task>;
            if (action1 != null)
                await action1(context);
        }

        public async Task Step2(StepContext context)
        {
            if (_step2 != null)
                await this._step2(context);
            var action1 = context.InstanceItems["InstanceCall_Step2"] as Func<StepContext, Task>;
            if (action1 != null)
                await action1(context);
        }

        public async Task Step3(StepContext context)
        {
            if (_step3 != null)
                await this._step3(context);
            var action1 = context.InstanceItems["InstanceCall_Step3"] as Func<StepContext, Task>;
            if (action1 != null)
                await action1(context);
        }

        public async Task Step4(StepContext context)
        {
            if (_step4 != null)
                await this._step4(context);
            var action1 = context.InstanceItems["InstanceCall_Step4"] as Func<StepContext, Task>;
            if (action1 != null)
                await action1(context);
        }
    }
}
