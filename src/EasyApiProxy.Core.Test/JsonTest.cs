using System.Globalization;
using System.Net.Security;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Tests;

[TestFixture]
public class JsonTest : BaseTest
{
    [Test]
    public void Json001()
    {
        var opt = new JsonSerializerOptions();
        opt.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opt.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

        opt.Converters.Add(new DateTimeConverter());

        var info = new Info {
            Name = "David",
            Prop1 = (Object?) null,
            BirthDate = new DateTime(1974, 09, 19),
            CreateDate = new DateTime(2000, 1, 31, 12, 30, 15, 123)
        };

        //JsonSerializer.Serialize(info, info.GetType(), opt);
        var txt = JsonSerializer.Serialize(info,  opt);

        var info2 = JsonSerializer.Deserialize<Info>(txt, opt);

        Assert.That(info2?.BirthDate, Is.EqualTo(info.BirthDate));
        Assert.That(info2?.CreateDate.Date, Is.EqualTo(info.CreateDate.Date));
    }

    [Test]
    public void Json002() {

        var dictionary = new Dictionary<string, JsonNode?>
        {
            ["name1"] = "value1",
            ["name2"] = 2
        };
        var jo1 = new JsonObject(dictionary);

        var txt1 = JsonSerializer.Serialize(jo1);



        var jo2 = new JsonObject
                    {
                        ["Hello"] = "world",
                        ["Name"] = 123,
                    };
        var txt2 = JsonSerializer.Serialize(jo2);
    }

    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss", CultureInfo.InvariantCulture));
        }
    }

    private class Info
    {
        public string? Name { get; set; }
        public object? Prop1 { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}