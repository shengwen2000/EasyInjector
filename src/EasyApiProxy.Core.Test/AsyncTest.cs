using EasyApiProxys;
using EasyApiProxys.DemoApis;
using System.Threading;
using System.Threading.Tasks;

namespace Tests;

[TestFixture]
public class AsyncTest : BaseTest
{
     /// <summary>
        /// winform 環境 無窮等待 現象
        /// </summary>
        [Test, Apartment(ApartmentState.STA)]
        public async Task Async001_NG()
        {
            // winform之類的會有 SynchronizationContext
            Assert.IsNotNull(SynchronizationContext.Current);
            await Task.Delay(100);

            // 可以正常執行
            Action1_OK().GetAwaiter().GetResult();

            // 可以正常執行
            Task.Run(() => Action1_NG()).GetAwaiter().GetResult();

            // 無窮等待
            // 簡單的說 Action1 執行時 會將結果附加回 目前的Context，又自己等待自己導致
            Action1_NG().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 非winform 環境 一切正常
        /// </summary>
        [Test]
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

        async Task<int> Action1_NG()
        {
            await Task.Delay(1000);
            return 123;
        }

}
