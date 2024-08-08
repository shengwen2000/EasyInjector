using EasyApiProxys;
using EasyApiProxys.DemoApis;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
	[Category("Others")]
	[TestFixture]
    public class AsyncTest : BaseTest
	{
        /// <summary>
        /// winform ���� �L�a���� �{�H
        /// </summary>
        [Test]
        public async void Async001_NG()
        {
            // winform�������|�� SynchronizationContext
            Assert.IsNotNull(SynchronizationContext.Current);
            await Task.Delay(100);

            // �i�H���`����
            Action1_OK().GetAwaiter().GetResult();          

            // �L�a���� 
            // ²�檺�� Action1 ����� �|�N���G���[�^ �ثe��Context�A�S�ۤv���ݦۤv�ɭP
            Action1_NG().GetAwaiter().GetResult();
        }

        /// <summary>
        /// �Dwinform ���� �@�����`
        /// </summary>
        [Test]
        public async Task Async001_OK()
        {
            // �Dwinform ���� ���|�� SynchronizationContext
            Assert.IsNull(SynchronizationContext.Current);
            await Task.Delay(100);

            // �i�H���`����
            Action1_OK().GetAwaiter().GetResult();

            // �i�H���`����      
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
}