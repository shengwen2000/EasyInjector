using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class BaseTest
    {
        static Random rnd = new Random();

        public BaseTest()
        {
            Console.SetOut(new ToDebugWriter());
        }

        /// <summary>
        /// 產生隔離代號
        /// </summary>
        protected string GenerateIsolateNumber()
        {
            // var bb = Guid.NewGuid().ToByteArray();
            // var hash = Crc32Algorithm.Compute(bb).ToString("x8");
            var num = rnd.Next(100000, 1000000);
            return string.Format("{0:ddHHmmss}A{1}", DateTime.Now, num);
        }

        class ToDebugWriter : StringWriter
        {
            public override void WriteLine(string value)
            {
                Debug.WriteLine(value);
            }

            public override void WriteLine()
            {
                Debug.WriteLine(string.Empty);
            }

            public override void Write(string value)
            {
                Debug.Write(value);
            }
        }
    }
}
