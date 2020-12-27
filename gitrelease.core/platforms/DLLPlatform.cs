namespace gitrelease.platforms
{
    internal class DLLPlatform : IPlatform
    {
        private string path;

        public PlatformType Type { get; } = PlatformType.DLL;

        public DLLPlatform(string path)
        {
            this.path = path;
        }

        public ReleaseSequenceFlags Release()
        {
            return ReleaseSequenceFlags.Ok;
        }
    }
}