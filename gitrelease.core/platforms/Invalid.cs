using System;

namespace gitrelease.platforms
{
    internal class Invalid : IPlatform
    {
        public PlatformType Type { get; } = PlatformType.INVALID;

        public Invalid(string path)
        {
        }

        public ReleaseSequenceFlags Release()
        {
            throw new NotSupportedException();
        }
    }
}