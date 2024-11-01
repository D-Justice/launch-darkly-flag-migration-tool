using Newtonsoft.Json;

namespace LaunchDarklyMigrationTool.Models.Pipelines
{
    public class ReleasePipelineResponse
    {
        [JsonProperty("items")]
        public List<Item> Items { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }
    }
    public class Item
    {
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("phases")]
        public List<Phase> Phases { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("_version")]
        public int Version { get; set; }

        [JsonProperty("isProjectDefault")]
        public bool IsProjectDefault { get; set; }

        [JsonProperty("_isLegacy")]
        public bool IsLegacy { get; set; }
    }
    public class Audience
    {
        [JsonProperty("environment")]
        public Environment Environment { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("configuration")]
        public Configuration Configuration { get; set; }
    }

    public class Configuration
    {
        [JsonProperty("releaseStrategy")]
        public string ReleaseStrategy { get; set; }

        [JsonProperty("requireApproval")]
        public bool RequireApproval { get; set; }

        [JsonProperty("notifyTeamKeys")]
        public List<string> NotifyTeamKeys { get; set; }
    }

    public class Environment
    {
        [JsonProperty("_links")]
        public Links Links { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }
    }

    

    public class Links
    {
        [JsonProperty("self")]
        public Self Self { get; set; }

        [JsonProperty("site")]
        public Site Site { get; set; }
    }

    public class Phase
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("audiences")]
        public List<Audience> Audiences { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("configuration")]
        public Configuration Configuration { get; set; }
    }


    public class Self
    {
        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Site
    {
        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
