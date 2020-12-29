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

        public ReleaseManagerFlags Release(string version)
        {
            return ReleaseManagerFlags.Ok;
        }

        public string GetVersion()
        {
            return "";
        }
    }
}