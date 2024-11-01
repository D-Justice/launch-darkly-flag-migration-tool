namespace LaunchDarklyMigrationTool.Models.Flags
{
    public class CreateFlagRequest
    {
        public ClientSideAvailability ClientSideAvailability { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<Variation> Variations { get; set; }
        public bool Temporary { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public string MaintainerId { get; set; }
        public string MaintainerTeamKey { get; set; }
        

    }

    public class ClientSideAvailability
    {
        public bool UsingEnvironmentId { get; set; }
        public bool UsingMobileKey { get; set; }
    }
}
