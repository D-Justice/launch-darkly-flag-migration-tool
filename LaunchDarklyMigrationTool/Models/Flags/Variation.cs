using Newtonsoft.Json;

namespace LaunchDarklyMigrationTool.Models.Flags
{
    public class Variation
    {
        [JsonProperty("_id")]
        public string Id { get; set; }
        public dynamic Value { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
    }
}
