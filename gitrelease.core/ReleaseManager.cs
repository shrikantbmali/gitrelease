using gitrelease.platforms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gitrelease
{
    internal class ReleaseManager : IReleaseManager
    {
        private ConfigFile? file = null;

        private IEnumerable<IPlatform> platforms;

        public ReleaseManager(ConfigFile file)
        {
            this.file = file;
        }

        public ReleaseManagerFlags Initialize()
        {
            if (this.file == null)
            {
                return ReleaseManagerFlags.InvalidFile;
            }

            this.platforms = CreatePlatforms(this.file.Value.Platforms).ToArray();

            if (this.platforms.Any(platform => platform.Type == PlatformType.INVALID))
            {
                return ReleaseManagerFlags.InvalidPlatform;
            }

            return ReleaseManagerFlags.Ok;
        }

        public ReleaseSequenceFlags Release()
        {
            return ExecuteSafe(() =>
            {
                return GitSanity(() =>
                {
                    return ReleaseSequenceFlags.Ok;
                });
            }, ReleaseSequenceFlags.Unknown);
        }

        private static ReleaseSequenceFlags GitSanity(Func<ReleaseSequenceFlags> func)
        {
            return func();
        }

        private static ReleaseSequenceFlags ExecuteSafe(Func<ReleaseSequenceFlags> func, ReleaseSequenceFlags failFlag)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return failFlag;
            }
        }

        private static IEnumerable<IPlatform> CreatePlatforms(IEnumerable<Platform> platforms)
        {
            foreach (var platform in platforms)
            {
                switch (platform.Name?.ToLower())
                {
                    case "dll":
                        yield return new DLLPlatform(platform.Path);
                        break;
                    case "ios":
                        yield return new IOSPlatform(platform.Path);
                        break;
                    case "droid":
                        yield return new DroidPlatform(platform.Path);
                        break;
                    case "uwp":
                        yield return new UWPPlatform(platform.Path);
                        break;
                    default:
                        yield return new Invalid(platform.Path);
                        break;
                }
            }
        }
    }
}