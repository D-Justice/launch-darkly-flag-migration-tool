namespace LaunchDarklyMigrationTool.Models.Project
{

        public class CreateProjectRequest
        {
            public string Name { get; set; }
            public string Key { get; set; }
            public IEnumerable<string> Tags { get; set; }
            public IEnumerable<EnvironmentRequest> Environments { get; set; }
            public NamingConventionRequest NamingConvention { get; set; }
        }

        public class EnvironmentRequest
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

        public class NamingConventionRequest
        {
            public string Case { get; set; }
        }

}
