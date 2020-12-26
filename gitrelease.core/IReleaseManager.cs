namespace gitrelease
{
    public interface IReleaseManager
    {
        ReleaseManagerFlags Initialize();
        
        ReleaseSequenceFlags Release();
    }
}
