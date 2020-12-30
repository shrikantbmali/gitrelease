namespace gitrelease.core.platforms
{
    internal class UWPPlatform : IPlatform
    {
        private string path;

        public PlatformType Type { get; } = PlatformType.UWP;
        
        public UWPPlatform(string path)
        {
            this.path = path;
        }

        public (ReleaseManagerFlags flag, string[] changedFiles) Release(string version)
        {
            return (ReleaseManagerFlags.Ok, new string[] { });
        }

        public string GetVersion()
        {
            return "";
        }
    }
}