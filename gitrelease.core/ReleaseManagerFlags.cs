namespace gitrelease.core
{
    public enum ReleaseManagerFlags : int
    {
        Ok = 0,
        Unknown = int.MinValue,
        InvalidFile = -1,
        InvalidPlatform = -2,
        InvalidRootDir = -3,
        ConfigurationNotFound = -4,
        DirtyRepo = -5,
        InvalidDirectory = -6,
        FileNotFound = -7,
        Cancelled = -8,
        ConfigFileAlreadyExists = -9
    }
}