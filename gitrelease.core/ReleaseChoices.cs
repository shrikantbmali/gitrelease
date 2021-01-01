namespace gitrelease.core
{
    public class ReleaseChoices
    {
        public ReleaseType ReleaseType { get; }
    }

    public enum ReleaseType
    {
        Major,
        Minor,
        Patch,
        Custom
    }
}