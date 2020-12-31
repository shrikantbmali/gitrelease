using System;
using System.IO;
using LibGit2Sharp;

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
            var initResult = InstallTools();

            if (initResult != ReleaseManagerFlags.Ok)
                return initResult;
                
            var configFilePath = Path.Combine(_root, ConfigFile.FixName);

            if (File.Exists(configFilePath))
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

            if (configFile.Save(configFilePath))
            {
                var command = Path.Combine(_root, @".\nbgv.exe");

                var result = CommandExecutor.ExecuteFile(command, $"install -p {_root}");

                if (result.isError)
                    return ReleaseManagerFlags.FailedToInstallNBGV;

                using var repository = new Repository(_root);
                repository.Index.Add(Path.GetFileName(configFilePath));
                repository.Index.Add("package.json");
                repository.Index.Add("package-lock.json");
                repository.Index.Write();

                return ReleaseManagerFlags.Ok;
            }
            else
            {
                return ReleaseManagerFlags.Unknown;
            }
        }

        private ReleaseManagerFlags InstallTools()
        {
            var (output, isError) = CommandExecutor.ExecuteCommand("dotnet", $"tool install --tool-path {_root} nbgv --version 3.3.37", _root);

            if (isError)
            {
                Console.WriteLine(output);
                return ReleaseManagerFlags.FailedToInstallNBGV;
            }

            (output, isError) = CommandExecutor.ExecuteCommand("npm install", "auto-changelog", _root);

            if (isError)
            {
                Console.WriteLine(output);
                return ReleaseManagerFlags.FailedToInstallAutoChangelLog;
            }

            return ReleaseManagerFlags.Ok;
        }
    }
}