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

        public (ReleaseManagerFlags flag, string[] changedFiles) Release(string version)
        {
            return (ReleaseManagerFlags.Ok, new string[]{});
        }

        public string GetVersion()
        {
            return "";
        }
    }
}