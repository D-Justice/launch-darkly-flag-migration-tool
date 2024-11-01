using Newtonsoft.Json;

namespace LaunchDarklyMigrationTool.Models.Pipelines
{
    public class ReleasePipelineRequestWrapper
    {
        public IEnumerable<ReleasePipelineRequest> ReleasePipelineRequests { get; set; }
    }
    public class ReleasePipelineRequest
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("phases")]
        public List<PhaseRequest> PhaseRequests { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("isProjectDefault")]
        public bool IsProjectDefault { get; set; }

        [JsonProperty("isLegacy")]
        public bool IsLegacy { get; set; }
    }
    public class AudienceRequest
    {
        [JsonProperty("environmentKey")]
        public string EnvironmentKey { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("segmentKeys")]
        public List<string> SegmentKeys { get; set; }

        [JsonProperty("configuration")]
        public ConfigurationRequest ConfigurationRequest { get; set; }
    }

    public class ConfigurationRequest
    {
        [JsonProperty("releaseStrategy")]
        public string ReleaseStrategy { get; set; }

        [JsonProperty("requireApproval")]
        public bool RequireApproval { get; set; }

        [JsonProperty("notifyMemberIds")]
        public List<string> NotifyMemberIds { get; set; }

        [JsonProperty("notifyTeamKeys")]
        public List<string> NotifyTeamKeys { get; set; }

        [JsonProperty("releaseGuardianConfiguration")]
        public ReleaseGuardianConfigurationRequest ReleaseGuardianConfigurationRequest { get; set; }

        [JsonProperty("bakeTimeDurationMs")]
        public int BakeTimeDurationMs { get; set; }
    }

    public class PhaseRequest
    {
        [JsonProperty("audiences")]
        public List<AudienceRequest> AudienceRequests { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("configuration")]
        public ConfigurationRequest ConfigurationRequest { get; set; }
    }

    public class ReleaseGuardianConfigurationRequest
    {
        [JsonProperty("monitoringWindowMilliseconds")]
        public int MonitoringWindowMilliseconds { get; set; }

        [JsonProperty("rolloutWeight")]
        public int RolloutWeight { get; set; }

        [JsonProperty("rollbackOnRegression")]
        public bool RollbackOnRegression { get; set; }

        [JsonProperty("randomizationUnit")]
        public string RandomizationUnit { get; set; }
    }
}
