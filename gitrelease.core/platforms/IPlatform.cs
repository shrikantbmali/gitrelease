namespace gitrelease.core.platforms
{
    internal interface IPlatform
    {
        PlatformType Type { get; }

        ReleaseManagerFlags Release(string version);

        string GetVersion();
    }
}