namespace LaunchDarklyMigrationTool.Models.Project
{
    public class GetProjectResponse
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public Environment Environments { get; set; }
        public NamingConvention NamingConvention { get; set; }
    }

    public class Environment
    {
        public List<Item>? Items { get; set; }
    }

    public class Item
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string Color { get; set; }
        public int DefaultTtl { get; set; }
        public bool SecureMode { get; set; }
        public bool DefaultTrackEvents { get; set; }
        public bool ConfirmChanges { get; set; }
        public bool RequireComments { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public bool Critical { get; set; }
    }

    public class NamingConvention
    {
        public string Case { get; set; }
    }
}
