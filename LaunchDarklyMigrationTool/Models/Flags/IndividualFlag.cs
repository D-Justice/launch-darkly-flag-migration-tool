using Newtonsoft.Json;

namespace LaunchDarklyMigrationTool.Models.Flags
{
    public class IndividualFlag
    {
        [JsonProperty("environments")]
        public Dictionary<string, IndividualFlagEnvironmentData> Environments { get; set; }
        public List<Variation> Variations { get; set; }
    }

    public class IndividualFlagEnvironmentData
    {
        public List<RuleInformation> rules { get; set; }
        public FallThrough fallThrough { get; set; }
        public int OffVariation { get; set; }
        public List<PreRequisites> PreRequisites { get; set; }
    }

    public class PreRequisites
    {
        public string Key { get; set; }
        public int Variation { get; set; }
    }
    public class RuleInformation
    {
        public List<Clause> Clauses { get; set; }
        [JsonProperty("_id")]
        public string ID { get; set; }
        public int Variation { get; set; }
        public bool TrackEvents { get; set; }
        public string Description { get; set; }
    }

    public class Clause
    {
        [JsonProperty("_id")]
        public string ID { get; set; }
        public string Attribute { get; set; }
        public string Op { get; set; }
        public List<dynamic> Values { get; set; }
        public string ContextKind { get; set; }
        public bool Negate { get; set; }
    }

    public class FallThrough
    {
        public int Variation { get; set; }
    }
}
