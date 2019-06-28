using Newtonsoft.Json;

namespace SFMC4NET.Entities
{
    public partial class Asset
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("CustomerKey")]
        public string CustomerKey { get; set; }

        [JsonProperty("AssetType")]
        public AssetType AssetType { get; set; }

        [JsonProperty("Category")]
        public CategoryId Category { get; set; }

        [JsonProperty("File")]
        public string File { get; set; }
    }

    public partial class CategoryId
    {
        [JsonProperty("id")]
        public long Id { get; set; }
    }

    public partial class AssetType
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Id")]
        public long Id { get; set; }
    }
}
