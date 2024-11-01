using Newtonsoft.Json;

namespace LaunchDarklyMigrationTool.Models.Flags
{
    public class GetFlagListResponse
    {
        [JsonProperty("items")]
        public IEnumerable<Flag> Flags { get; set; }
        public int TotalCount { get; set; }
    }

    public class Flag
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public bool Temporary { get; set; }
        public List<Variation> Variations { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public bool Deprecated { get; set; }
        public bool Archived { get; set; }
        [JsonProperty("_links")]
        public Link Links { get; set; }
        public string MaintainerId { get; set; }
        [JsonProperty("_maintainerTeam")]
        public MaintainerTeam MaintainerTeam { get; set; }
        public CustomProperties CustomProperties { get; set; }
        public Defaults Defaults { get; set; }
        public ClientSideAvailability ClientSideAvailability { get; set; }
        public long CreationDate { get; set; }
        public long DeprecatedDate { get; set; }
        public Experiments Experiments { get; set; }
        [JsonProperty("environments")]
        public Dictionary<string, EnvironmentData> EnvironmentsJson { get; set; }
        [JsonIgnore]
        public IndividualFlag ExistingFlagInformation { get; set; }
    }

    public class Experiments
    {
        public int BaseLineIdx { get; set; }
        public IEnumerable<ExperimentItem> items { get; set; }
    }
    //TODO: Fill out later
    public class ExperimentItem
    {
        public string MetricKey { get; set; }
    }
    public class MaintainerTeam
    {
        public IEnumerable<Link> Links { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
    }
    public class Defaults
    {
        public int OnVariation { get; set; }
        public int OffVariation { get; set; }
    }
    public class CustomProperties
    {
        [JsonProperty("jira.issues")]
        public CustomPropertiesDetails JiraIssues { get; set; }
    }

    public class CustomPropertiesDetails
    {
        public string Name { get; set; }
        public IEnumerable<string>? Values { get; set; } 
    }
    public class Link
    {
        [JsonProperty("parent")]
        public LinkDetail Parent { get; set; }
        [JsonProperty("self")]
        public LinkDetail Self { get; set; }
    }

    public class MaintainerLinks : Link
    {
        [JsonProperty("roles")]
        public LinkDetail Roles { get; set; }
        [JsonProperty("maintainers")]
        public LinkDetail Maintainers { get; set; }
    }

    public class LinkDetail
    {
        public string Href { get; set; }
        public string Type { get; set; }
    }
}
