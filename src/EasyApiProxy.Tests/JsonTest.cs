using EasyApiProxys;
using EasyApiProxys.DemoApis;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Tests
{
	[Category("Others")]
	[TestFixture]
    public class JsonTest : BaseTest
	{
        [Test]
        public void Json001()
        {
            var setting = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Unspecified,
                //ContractResolver = new DefaultContractResolver()
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var js = JsonSerializer.Create(setting);

            var ret = new DefaultApiResult { Result="OK", Message="Success", Data= new { Hello_World = "123", Today = DateTime.Now }};

            var txt = JsonConvert.SerializeObject(ret, setting);

            var sw = new StringWriter();
            var jw = new JsonTextWriter(sw);
            js.Serialize(jw, ret);
            jw.Flush();
            var txt2 = sw.ToString();
        }       
	}
}