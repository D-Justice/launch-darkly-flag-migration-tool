namespace LaunchDarklyMigrationTool.Models.Flags
{
    using System.Collections.Generic;

    public class EnvironmentData
    {
        public string _environmentName { get; set; }
        public Site _site { get; set; }
        public Summary _summary { get; set; }
        public bool archived { get; set; }
        public long lastModified { get; set; }
        public bool on { get; set; }
        public string salt { get; set; }
        public string sel { get; set; }
        public bool trackEvents { get; set; }
        public bool trackEventsFallthrough { get; set; }
        public int version { get; set; }
    }

    public class Site
    {
        public string href { get; set; }
        public string type { get; set; }
    }

    public class Summary
    {
        public int prerequisites { get; set; }
        public Dictionary<string, VariationData> variations { get; set; }
    }

    public class VariationData
    {
        public int contextTargets { get; set; }
        public bool isFallthrough { get; set; }
        public bool isOff { get; set; }
        public int nullRules { get; set; }
        public int rules { get; set; }
        public int targets { get; set; }
    }
}
