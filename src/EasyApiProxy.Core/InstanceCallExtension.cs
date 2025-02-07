using EasyApiProxys.Options;

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

    internal class InstanceHandler(
        Func<StepContext, Task>? step1,
        Func<StepContext, Task>? step2,
        Func<StepContext, Task>? step3,
        Func<StepContext, Task>? step4)
    {
        public async Task Step1(StepContext context)
        {
            if (step1 != null)
                await step1(context);
            if (context.InstanceItems["InstanceCall_Step1"] is Func<StepContext, Task> action1)
                await action1(context);
        }

        public async Task Step2(StepContext context)
        {
            if (step2 != null)
                await step2(context);
            if (context.InstanceItems["InstanceCall_Step2"] is Func<StepContext, Task> action1)
                await action1(context);
        }

        public async Task Step3(StepContext context)
        {
            if (step3 != null)
                await step3(context);
            if (context.InstanceItems["InstanceCall_Step3"] is Func<StepContext, Task> action1)
                await action1(context);
        }

        public async Task Step4(StepContext context)
        {
            if (step4 != null)
                await step4(context);
            if (context.InstanceItems["InstanceCall_Step4"] is Func<StepContext, Task> action1)
                await action1(context);
        }
    }
}
