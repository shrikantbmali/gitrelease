namespace gitrelease.core
{
    public class ReleaseChoices
    {
        public ReleaseType ReleaseType { get; set; }

        public GitVersion CustomVersion { get; set; }

        public bool DryRun { get; set; }

        public bool SkipTag { get; set; }

        public bool SkipChangelog { get; set; }

        public bool IgnoreDirty { get; set; }

        public int ChangelogCharacterLimit { get; set; }

        public ChangeLogType ChangeLogType { get; set; }
        
        public string ChangelogFileName { get; set; }
        
        public string AppendValue { get; set; }

        public bool SkipSign { get; set; }
    }

    public enum ChangeLogType
    {
        FirstTime,
        Incremental
    }

    public enum ReleaseType
    {
        Major,
        Minor,
        Patch,
        BuildNumber,
        Custom
    }
}