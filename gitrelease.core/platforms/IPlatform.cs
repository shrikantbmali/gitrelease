namespace gitrelease.platforms
{
    internal interface IPlatform
    {
        PlatformType Type { get; }

        ReleaseSequenceFlags Release();
    }
}