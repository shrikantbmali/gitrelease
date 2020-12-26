namespace gitrelease.platforms
{
    internal class IOSPlatform : IPlatform
    {
        private string path;

        public PlatformType Type { get; } = PlatformType.IOS;

        public IOSPlatform(string path)
        {
            this.path = path;
        }
    }
}