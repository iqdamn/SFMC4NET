using Newtonsoft.Json;

namespace SFMC4NET.Entities
{
    public partial class Asset
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("customerKey")]
        public string CustomerKey { get; set; }

        [JsonProperty("assetType")]
        public AssetType AssetType { get; set; }

        [JsonProperty("category")]
        public CategoryId Category { get; set; }        
    }

    public partial class AssetFile : Asset
    {
        [JsonProperty("File")]
        public string File { get; set; }
    }

    public partial class AssetEmail : Asset
    {
        [JsonProperty("channels")]
        public Channels channels { get; set; }

        [JsonProperty("views")]
        public Views views { get; set; }

        [JsonProperty("data")]
        public Data data { get; set; }
    }

    public partial class Channels
    {
        [JsonProperty("email")]
        public bool Email { get; set; }

        [JsonProperty("web")]
        public bool Web { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("email")]
        public EmailClass Email { get; set; }
    }

    public partial class EmailClass
    {
        [JsonProperty("options")]
        public Options Options { get; set; }
    }

    public partial class Options
    {
        [JsonProperty("characterEncoding")]
        public string CharacterEncoding { get; set; }
    }

    public partial class Views
    {
        [JsonProperty("html")]
        public Html Html { get; set; }

        [JsonProperty("subjectline")]
        public Html Subjectline { get; set; }

        [JsonProperty("preheader")]
        public Html Preheader { get; set; }
    }

    public partial class Html
    {
        [JsonProperty("content")]
        public string Content { get; set; }
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
