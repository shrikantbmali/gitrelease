using System;
using System.IO;

namespace gitrelease.core
{
    public interface IIniter
    {
        ReleaseManagerFlags Init();
    }

    internal class Initer : IIniter
    {
        private readonly string _root;
        private readonly Func<string, string?> _platformGetter;
        private readonly Func<string, string?> _pathGetter;

        public Initer(string root, Func<string, string?> platformGetter, Func<string, string?> pathGetter)
        {
            _root = root;
            _platformGetter = platformGetter;
            _pathGetter = pathGetter;
        }

        public ReleaseManagerFlags Init()
        {
            var filePath = Path.Combine(_root, ConfigFile.FixName);

            if (File.Exists(filePath))
            {
                return ReleaseManagerFlags.ConfigFileAlreadyExists;
            }

            var configFile = new ConfigFile
            {
                Platforms = new[]
                {
                    new Platform
                    {
                        Name = "ios",
                        Path = _pathGetter("Provide the path for iOS project folder.")

                    },
                    new Platform
                    {
                        Name = "droid",
                        Path = _pathGetter("Provide the path for Droid project folder.")

                    },
                    new Platform
                    {
                        Name = "uwp",
                        Path = _pathGetter("Provide the path for UWP project folder.")

                    },
                }
            };

            return configFile.Save(filePath) ? ReleaseManagerFlags.Ok : ReleaseManagerFlags.Unknown;
        }
    }
}