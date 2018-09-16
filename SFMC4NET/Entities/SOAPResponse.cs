// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var welcome = Welcome.FromJson(jsonString);

namespace SFMC4NET.Entities
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Root
    {
        [JsonProperty("?xml")]
        public Xml Xml { get; set; }

        [JsonProperty("soap:Envelope")]
        public SoapEnvelope SoapEnvelope { get; set; }
    }

    public partial class SoapEnvelope
    {
        [JsonProperty("@xmlns:soap")]
        public string XmlnsSoap { get; set; }

        [JsonProperty("@xmlns:xsi")]
        public string XmlnsXsi { get; set; }

        [JsonProperty("@xmlns:xsd")]
        public string XmlnsXsd { get; set; }

        [JsonProperty("@xmlns:wsa")]
        public string XmlnsWsa { get; set; }

        [JsonProperty("@xmlns:wsse")]
        public string XmlnsWsse { get; set; }

        [JsonProperty("@xmlns:wsu")]
        public string XmlnsWsu { get; set; }

        [JsonProperty("soap:Header")]
        public SoapHeader SoapHeader { get; set; }

        [JsonProperty("soap:Body")]
        public SoapBody SoapBody { get; set; }
    }

    public partial class SoapBody
    {
        [JsonProperty("RetrieveResponseMsg")]
        public RetrieveResponseMsg RetrieveResponseMsg { get; set; }
    }

    public partial class RetrieveResponseMsg
    {
        [JsonProperty("@xmlns")]
        public string Xmlns { get; set; }

        [JsonProperty("OverallStatus")]
        public string OverallStatus { get; set; }

        [JsonProperty("RequestID")]
        public Guid RequestId { get; set; }

        [JsonProperty("Results")]
        public Result[] Results { get; set; }
    }

    public partial class Result
    {
        [JsonProperty("@xsi:type")]
        public XsiTypeEnum XsiType { get; set; }

        [JsonProperty("PartnerKey")]
        public ObjectId PartnerKey { get; set; }

        [JsonProperty("ObjectID")]
        public ObjectId ObjectId { get; set; }

        [JsonProperty("Type")]
        public XsiTypeEnum Type { get; set; }

        [JsonProperty("Properties")]
        public Properties Properties { get; set; }
    }

    public partial class ObjectId
    {
        [JsonProperty("@xsi:nil")]
        [JsonConverter(typeof(ParseStringConverter))]
        public bool XsiNil { get; set; }
    }

    public partial class Properties
    {
        [JsonProperty("Property")]
        public Property[] Property { get; set; }
    }

    public partial class Property
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }
    }

    public partial class SoapHeader
    {
        [JsonProperty("wsa:Action")]
        public string WsaAction { get; set; }

        [JsonProperty("wsa:MessageID")]
        public string WsaMessageId { get; set; }

        [JsonProperty("wsa:RelatesTo")]
        public string WsaRelatesTo { get; set; }

        [JsonProperty("wsa:To")]
        public string WsaTo { get; set; }

        [JsonProperty("wsse:Security")]
        public WsseSecurity WsseSecurity { get; set; }
    }

    public partial class WsseSecurity
    {
        [JsonProperty("wsu:Timestamp")]
        public Dictionary<string, string> WsuTimestamp { get; set; }
    }

    public partial class Xml
    {
        [JsonProperty("@version")]
        public string Version { get; set; }

        [JsonProperty("@encoding")]
        public string Encoding { get; set; }
    }

    public enum Name { Circuit, Code, Day, Firstname, Lastname, Redeemer, RefId };

    public enum XsiTypeEnum { DataExtensionObject };

    public partial class Root
    {
        public static Root FromJson(string json) => JsonConvert.DeserializeObject<Root>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Root self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                XsiTypeEnumConverter.Singleton,
                NameConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class XsiTypeEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(XsiTypeEnum) || t == typeof(XsiTypeEnum?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "DataExtensionObject")
            {
                return XsiTypeEnum.DataExtensionObject;
            }
            throw new Exception("Cannot unmarshal type XsiTypeEnum");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (XsiTypeEnum)untypedValue;
            if (value == XsiTypeEnum.DataExtensionObject)
            {
                serializer.Serialize(writer, "DataExtensionObject");
                return;
            }
            throw new Exception("Cannot marshal type XsiTypeEnum");
        }

        public static readonly XsiTypeEnumConverter Singleton = new XsiTypeEnumConverter();
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(bool) || t == typeof(bool?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            bool b;
            if (Boolean.TryParse(value, out b))
            {
                return b;
            }
            throw new Exception("Cannot unmarshal type bool");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (bool)untypedValue;
            var boolString = value ? "true" : "false";
            serializer.Serialize(writer, boolString);
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }

    internal class NameConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Name) || t == typeof(Name?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Circuit":
                    return Name.Circuit;
                case "Code":
                    return Name.Code;
                case "Day":
                    return Name.Day;
                case "Firstname":
                    return Name.Firstname;
                case "Lastname":
                    return Name.Lastname;
                case "Redeemer":
                    return Name.Redeemer;
                case "RefId":
                    return Name.RefId;
            }
            throw new Exception("Cannot unmarshal type Name");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Name)untypedValue;
            switch (value)
            {
                case Name.Circuit:
                    serializer.Serialize(writer, "Circuit");
                    return;
                case Name.Code:
                    serializer.Serialize(writer, "Code");
                    return;
                case Name.Day:
                    serializer.Serialize(writer, "Day");
                    return;
                case Name.Firstname:
                    serializer.Serialize(writer, "Firstname");
                    return;
                case Name.Lastname:
                    serializer.Serialize(writer, "Lastname");
                    return;
                case Name.Redeemer:
                    serializer.Serialize(writer, "Redeemer");
                    return;
                case Name.RefId:
                    serializer.Serialize(writer, "RefId");
                    return;
            }
            throw new Exception("Cannot marshal type Name");
        }

        public static readonly NameConverter Singleton = new NameConverter();
    }
}
