using Newtonsoft.Json;
using UnityEngine;

namespace Yle.Fi
{
    public class TVProgramData
    {
        [JsonProperty("data")] public ContentData[] Data;
    }

    public class Title
    {
        [JsonProperty("fi")] public string Finnish;
        [JsonProperty("sv")] public string Swedish;

        public string Value
        {
            get
            {
                if (!string.IsNullOrEmpty(Finnish))
                    return Finnish;

                Debug.LogWarning("Finnish localization is empty, replacing with Swedish");
                return Swedish;
            }
        }
    }

    public class ItemTitle
    {
        [JsonProperty("und")] public string Value;
    }

    public class Creator
    {
        [JsonProperty("name")] public string Name;
        [JsonProperty("type")] public string Type;
    }

    public class ContentData
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("alternativeId")] public string[] AlternativeIds;

        [JsonProperty("itemTitle")] public ItemTitle ItemTitle;

        [JsonProperty("title")] public Title Title;
        [JsonProperty("type")] public string Type;
        [JsonProperty("description")] public Title Description;
        [JsonProperty("episodeNumber")] public int EpisodeNumber;
        [JsonProperty("creator")] public Creator[] Creator;
    }
}