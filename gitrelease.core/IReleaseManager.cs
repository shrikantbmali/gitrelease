namespace gitrelease.core
{
    public interface IReleaseManager
    {
        ReleaseManagerFlags Initialize();
        
        ReleaseManagerFlags Release();

        string[] GetVersion(string platformName);
    }
}
