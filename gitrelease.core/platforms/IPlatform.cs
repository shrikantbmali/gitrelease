namespace gitrelease.core.platforms
{
    internal interface IPlatform
    {
        PlatformType Type { get; }

        (ReleaseManagerFlags flag, string[] changedFiles) Release(string version);

        string GetVersion();
    }
}