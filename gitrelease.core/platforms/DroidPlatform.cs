namespace gitrelease.platforms
{
    internal class DroidPlatform : IPlatform
    {
        private string path;

        public PlatformType Type { get; } = PlatformType.Droid;
        
        public DroidPlatform(string path)
        {
            this.path = path;
        }
    }
}