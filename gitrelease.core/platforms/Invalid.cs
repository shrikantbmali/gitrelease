using System;

namespace gitrelease.core.platforms
{
    internal class Invalid : IPlatform
    {
        public PlatformType Type { get; } = PlatformType.INVALID;

        public Invalid(string path)
        {
        }

        public ReleaseManagerFlags Release(string version)
        {
            throw new NotSupportedException();
        }

        public string GetVersion()
        {
            throw new NotImplementedException();
        }
    }
}