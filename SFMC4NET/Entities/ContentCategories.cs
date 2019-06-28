using Newtonsoft.Json;
using System.Collections.Generic;

namespace SFMC4NET.Entities
{
    public partial class ContentCategories
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("page")]
        public long Page { get; set; }

        [JsonProperty("pageSize")]
        public long PageSize { get; set; }

        [JsonProperty("items")]
        public List<Category> Items { get; set; }
    }

    public partial class Category : IFolder
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("enterpriseId")]
        public long EnterpriseId { get; set; }

        [JsonProperty("memberId")]
        public long MemberId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("parentId")]
        public long ParentId { get; set; }

        [JsonProperty("categoryType")]
        public string CategoryType { get; set; }

        [JsonIgnore]
        public List<IFolder> Folders { get; set; }
    }
}
