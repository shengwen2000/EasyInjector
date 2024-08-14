using EasyInjectors;
using EasyInjectors.Dev;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tests.OverrideGenerics;

namespace Tests
{
    [Category("Common")]
    [TestFixture]
    public class OverrideGenericTest : BaseTest
    {
        [SetUp]
        public void Init()
        {
            //每個測試方法執行前都會執行的動作
        }

        [Test]
        public async void OverrideGeneric001()
        {
            var injector = new EasyInjector();

            // add generic service
            injector.AddGenericService(SimpleLifetimes.Singleton, typeof(ILogger<>), typeof(DefaultLogger<>));

            // 複寫測試類別
            injector.AddOverrideGeneric(typeof(ILogger<>), typeof(ConsoleLogger<>));

            var logger = injector.GetRequiredService<ILogger<Log>>();
            logger.AddLog(new Log { Message = "Demo1" });

            Assert.True(Log.Messages[0] == "Demo1");
            Assert.True(Log.Messages[1] == "(Console)Demo1");
            Log.Messages.Clear();

            await logger.AddLogAsync(new Log { Message = "Demo2" });
            Assert.True(Log.Messages[0] == "(Console Async)Demo2");
        }        
    }

    namespace OverrideGenerics
    {
        public class Log
        {
            public static List<string> Messages = new List<string>(10);

            public string Message { get; set; }

            public override string ToString()
            {
                return Message;
            }
        }

        public interface ILogger<TLOG> where TLOG:Log
        {
            void AddLog(TLOG log);

            Task AddLogAsync(TLOG log);
        }

        public class DefaultLogger<TLOG> : ILogger<TLOG> where TLOG : Log
        {
            public void AddLog(TLOG log)
            {
                Log.Messages.Add(log.ToString());
            }


            public async Task AddLogAsync(TLOG log)
            {
                await Task.Delay(500);
                Log.Messages.Add(string.Format("(Async){0}", log.ToString()));
            }
        }

        public class ConsoleLogger<TLOG> : ILogger<TLOG> where TLOG : Log
        {
            private ILogger<TLOG> _baseLogger;

            public ConsoleLogger(ILogger<TLOG> baseLogger)
            {
                _baseLogger = baseLogger;
            }

            [Override]
            public void AddLog(TLOG log)
            {
                _baseLogger.AddLog(log);
                Log.Messages.Add(string.Format("(Console){0}", log.ToString()));
            }


            [Override]
            public async Task AddLogAsync(TLOG log)
            {
                await Task.Delay(500);
                Log.Messages.Add(string.Format("(Console Async){0}", log.ToString()));
            }
        }
    }
}
