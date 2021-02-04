namespace gitrelease.core
{
    public enum ReleaseManagerFlags : int
    {
        Ok = 0,
        Unknown = int.MinValue,
        DirtyRepo = -5,
        InvalidDirectory = -6,
        FileNotFound = -7,
        TagCreationFailed = -12,
        ChangelogCreationFailed = -13,
        PackageJsonVersionUpdateFailed = -14,
        InvalidUWPPackageFile = -15,
        RepoNotInitializedForReleaseProcess = -19,
        InstallNpm = -20,
        AlreadyInitialized = -21,
        InvalidConfigFile = -22,
        InvalidAndroidManifestFileMissingAttributes = -23,
        RepoAlreadyInitialized = -24,
        NPMInitFailed = -25,
        NBGVInstallationFailed = -26,
        NBGVInitFailed = -27,
        ChangelogGeneratorInstallFailed = -28,
        ConfigFileCreationFailed = -29
    }
}