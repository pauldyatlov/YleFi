using Newtonsoft.Json;

namespace Yle.Fi
{
    public class TVProgramData
    {
        [JsonProperty("data")] public ContentData[] Data;
    }

    public class ItemTitle
    {
        [JsonProperty("und")] public string Value;
    }

    public class ContentData
    {
        [JsonProperty("typeMedia")] public string TypeMedia;
        [JsonProperty("alternativeId")] public string[] AlternativeIds;
        [JsonProperty("itemTitle")] public ItemTitle ItemTitle;
    }
}