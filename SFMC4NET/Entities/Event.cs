using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Globalization;

namespace SFMC4NET.Entities
{
    public partial class Event
    {
        [JsonProperty("ContactKey")]
        public string ContactKey { get; set; }

        [JsonProperty("EventDefinitionKey")]
        public string EventDefinitionKey { get; set; }

        [JsonProperty("Data")]
        public Dictionary<string, string> Data { get; set; }
    }

    public partial class Event
    {
        public static Event FromJson(string json) => JsonConvert.DeserializeObject<Event>(json, SFMC4NET.Entities.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Event self) => JsonConvert.SerializeObject(self, SFMC4NET.Entities.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}