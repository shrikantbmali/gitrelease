namespace gitrelease.platforms
{
    internal class UWPPlatform : IPlatform
    {
        private string path;

        public PlatformType Type { get; } = PlatformType.UWP;
        
        public UWPPlatform(string path)
        {
            this.path = path;
        }
    }
}