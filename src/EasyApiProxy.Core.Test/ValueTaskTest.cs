using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class ValueTaskTest
    {
        [Test]
        public async Task ValueTaskTest001() {

            var t1 = DoSomething1();
            var t2 = DoSomething2();

            var t1a = t1.AsTask();


            var t2a = ToValueTask(t2);


            await DoSomething1();
            await DoSomething2();        
        }

        public async ValueTask DoSomething1()
        {
            await Task.Delay(1000);
        }

        public async Task DoSomething2()
        {
            await Task.Delay(1000);
        }

        /// <summary>
        /// 轉換Task回傳類型 (轉換Task回傳類型)
        /// </summary>
        static async ValueTask<T> ToValueTask<T>(Task<T> a)
        {
            var r = await a;
            return r;
        }

        static async ValueTask ToValueTask(Task a)
        {
            await a;
        }


    }
}
