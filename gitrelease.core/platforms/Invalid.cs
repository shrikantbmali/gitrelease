using System;

namespace gitrelease.core.platforms
{
    internal class Invalid : IPlatform
    {
        public PlatformType Type { get; } = PlatformType.INVALID;

        public Invalid(string path)
        {
        }

        public (ReleaseManagerFlags flag, string[] changedFiles) Release(GitVersion version)
        {
            throw new NotSupportedException();
        }

        public string GetVersion()
        {
            throw new NotImplementedException();
        }
    }
}