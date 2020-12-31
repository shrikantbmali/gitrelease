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
        ConfigFileAlreadyExists = -9,
        ToolNotFound = -10,
        UnableToRollbackChangesDoneByNVGB = -11,
        TagCreationFailed = -12,
        ChangelogCreationFailed = -13,
        PackageJsonVersionUpdateFailed = -14,
        InvalidUWPPackageFile = -15,
        FailedToInstall = -16,
        FailedToInstallNBGV = -17,
        FailedToInstallAutoChangelLog = -18
    }
}