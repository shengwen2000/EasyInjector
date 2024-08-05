using EasyApiProxys;
using EasyApiProxys.DemoApis;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Tests
{
	[Category("EasyApiProxy")]
	[TestFixture]
    public class JsonTest : BaseTest
	{
        [Test]
        public async Task Json001()
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
            var txt = JsonConvert.SerializeObject(new { Hello_World = "123", Today = DateTime.Now }, setting);

        }       
	}
}