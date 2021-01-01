namespace gitrelease.core.platforms
{
    internal interface IPlatform
    {
        PlatformType Type { get; }

        (ReleaseManagerFlags flag, string[] changedFiles) Release(GitVersion version);

        string GetVersion();
    }
}