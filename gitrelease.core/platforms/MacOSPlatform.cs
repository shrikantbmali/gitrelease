namespace gitrelease.core.platforms;

class MacOSPlatform : IOSPlatform
{
    public override PlatformType Type { get; } = PlatformType.MacOs;

    public MacOSPlatform(string absolutePath) : base(absolutePath)
    {
        
    }

    public static bool IsValid(string path, string root)
    {
        return IOSPlatform.IsValid(path, root);
    }
}