namespace gitrelease.core.platforms
{
    internal class DLLPlatform : IPlatform
    {
        private string path;

        public PlatformType Type { get; } = PlatformType.DLL;

        public DLLPlatform(string path)
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