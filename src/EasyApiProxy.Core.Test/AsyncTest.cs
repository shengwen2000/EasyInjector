using EasyApiProxys;
using EasyApiProxys.DemoApis;
using System.Threading;
using System.Threading.Tasks;

namespace Tests;

[TestClass]
public class AsyncTest : BaseTest
{
    /// <summary>
    /// winform 環境 無窮等待 現象
    /// </summary>
    [TestMethod]
    public void Async001_NG()
    {

        //Task.Run(() => {}).ContinueWith((), new TaskScheduler())

        // 類視窗環境模擬
        // SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

        // Thread.CurrentThread.Name = "hello";
        // var scheduler = TaskScheduler.FromCurrentSynchronizationContext();

        // var task1 = Task.Factory.StartNew(
        //     () => Action1_NG(),
        //     CancellationToken.None,
        //     TaskCreationOptions.None,
        //     scheduler
        // );

        // task1.GetAwaiter().GetResult();

        // // winform之類的會有 SynchronizationContext
        // Assert.IsNotNull(SynchronizationContext.Current);
        // await Task.Delay(100);

        // // 可以正常執行
        // Action1_OK().GetAwaiter().GetResult();

        // // 可以正常執行
        // Task.Run(() => Action1_NG()).GetAwaiter().GetResult();

        // 無窮等待
        // 簡單的說 Action1 執行時 會將結果附加回 目前的Context，又自己等待自己導致
        //Action1_NG1().Wait();
    }

    /// <summary>
    /// 非winform 環境 一切正常
    /// </summary>
    [TestMethod]
    public async Task Async001_OK()
    {
        // 非winform 環境 不會有 SynchronizationContext
        Assert.IsNull(SynchronizationContext.Current);
        await Task.Delay(100);

        // 可以正常執行
        Action1_OK().GetAwaiter().GetResult();

        // 可以正常執行
        Action1_NG().GetAwaiter().GetResult();
    }

    async Task<int> Action1_OK()
    {
        await Task.Delay(1000).ConfigureAwait(false);
        return 123;
    }

    async Task Action1_NG1() {
        await Action1_NG();
        await Action1_NG();
    }

    async Task<int> Action1_NG()
    {
        var ctx1 = SynchronizationContext.Current ;
        await Task.Delay(1000).ConfigureAwait(true);
        var ctx = SynchronizationContext.Current ;
        Console.WriteLine(Thread.CurrentThread.Name);
        return 123;
    }
}
