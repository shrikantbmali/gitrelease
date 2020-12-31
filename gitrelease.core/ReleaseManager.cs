using gitrelease.core.platforms;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gitrelease.core
{
    internal class ReleaseManager : IReleaseManager
    {
        private ReadOnlyDictionary<string, IPlatform> _platforms;
        private readonly string _rootDirectory;

        public ReleaseManager(string rootDirectory)
        {
            _rootDirectory = rootDirectory;
        }

        public ReleaseManagerFlags Initialize()
        {
            if (!Directory.Exists(_rootDirectory))
            {
                return ReleaseManagerFlags.InvalidRootDir;
            }

            if (!ConfigFileExists(_rootDirectory, out var configFilePath))
            {
                return ReleaseManagerFlags.ConfigurationNotFound;
            }

            if (!TryParseFile(configFilePath, out var config))
            {
                return ReleaseManagerFlags.InvalidFile;
            }

            _platforms = CreatePlatforms(config.Platforms);

            if (_platforms.Any(platform => platform.Value.Type == PlatformType.INVALID))
            {
                return ReleaseManagerFlags.InvalidPlatform;
            }

            return ReleaseManagerFlags.Ok;
        }

        public ReleaseManagerFlags Release()
        {
            return Transaction(repo => GitSanity(repo, PrepareRelease), ReleaseManagerFlags.Unknown);
        }

        private ReleaseManagerFlags PrepareRelease(IRepository repo)
        {
            var head = repo.Head;

            var result = ReleaseManagerFlags.Unknown;

            try
            {
                var (output, isError) = ExecutePrepareRelease();

                if (isError)
                    return ReleaseManagerFlags.ToolNotFound;

                var releaseMessage = ReleaseMessage.FromJson(output);

                result = DeleteBranchAndRollbackCommitsDone(repo, head.Tip, releaseMessage);

                if (result != ReleaseManagerFlags.Ok)
                    return result;

                var version = $"{releaseMessage.NewBranch.Version}.{releaseMessage.NewBranch.Commit.Substring(0, 10)}";

                IEnumerable<(ReleaseManagerFlags flag, string[] changedFiles)> release =
                    _platforms.Select(platform => platform.Value.Release(version)).ToArray();

                result = release.FirstOrDefault(res => res.flag != ReleaseManagerFlags.Ok).flag;

                if (result == ReleaseManagerFlags.Ok)
                {
                    result = CreateATag(repo, version);
                    
                    if (result == ReleaseManagerFlags.Ok)
                    {
                        result = CreateChangeLog(version);

                        if (result == ReleaseManagerFlags.Ok)
                        {
                            result = Stage(repo);
                            if (result == ReleaseManagerFlags.Ok)
                            {
                                result = Commit(repo, version);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = ReleaseManagerFlags.Unknown;
                Console.WriteLine(ex);
            }
            finally
            {
                if (result != ReleaseManagerFlags.Ok)
                {
                    repo.Reset(ResetMode.Hard, head.Tip);
                }
            }
            
            return result;
        }

        private ReleaseManagerFlags CreateChangeLog(string version)
        {
            try
            {
                var result = UpdateJsonPackageVersion(version);

                if (result != ReleaseManagerFlags.Ok)
                    return result;

                var (_, isError) = CommandExecutor.ExecuteCommand("changelog", "generate", _rootDirectory);

                return isError ? ReleaseManagerFlags.ChangelogCreationFailed : ReleaseManagerFlags.Ok;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return ReleaseManagerFlags.ChangelogCreationFailed;
            }
        }

        private ReleaseManagerFlags UpdateJsonPackageVersion(string version)
        {
            try
            {
                var combine = Path.Combine(_rootDirectory, "package.json");
                var json = JObject.Parse(File.ReadAllText(combine));
                json["version"] = version;

                var js = JsonConvert.SerializeObject(json, Formatting.Indented);
                File.WriteAllText(combine, js);

                return ReleaseManagerFlags.Ok;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return ReleaseManagerFlags.PackageJsonVersionUpdateFailed;
            }
        }

        private static ReleaseManagerFlags CreateATag(IRepository repo, string version)
        {
            try
            {
                repo.ApplyTag($"v{version}");
                return ReleaseManagerFlags.Ok;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return ReleaseManagerFlags.TagCreationFailed;
            }
        }

        private (string output, bool isError) ExecutePrepareRelease()
        {
            var command = Path.Combine(_rootDirectory, @".\nbgv.exe");

            return CommandExecutor.ExecuteFile(command, $"prepare-release -p {_rootDirectory} -f json");
        }

        private static ReleaseManagerFlags DeleteBranchAndRollbackCommitsDone(IRepository repo, Commit head, ReleaseMessage releaseMessage)
        {
            try
            {
                repo.Branches.Remove(releaseMessage.NewBranch.Name);
                repo.Reset(ResetMode.Soft, head);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return ReleaseManagerFlags.UnableToRollbackChangesDoneByNVGB;
            }

            return ReleaseManagerFlags.Ok;
        }

        public string[] GetVersion(string platformName)
        {
            return platformName == "all"
                ? _platforms.Select(p => p.Value.GetVersion()).ToArray()
                : new[] { _platforms[platformName].GetVersion() };
        }

        private static bool ConfigFileExists(string rootPath, out string filePath)
        {
            filePath = Path.Combine(rootPath, ConfigFile.FixName);
            return File.Exists(filePath);
        }

        private static ReleaseManagerFlags GitSanity(Repository repo, Func<Repository, ReleaseManagerFlags> func)
        {
            var head = repo.Head;

            try
            {
                var status = repo.RetrieveStatus(new StatusOptions());

                return !status.IsDirty ? func(repo) : ReleaseManagerFlags.DirtyRepo;
            }
            catch (Exception ex)
            {
                repo.Reset(ResetMode.Hard, head.Tip);
                Console.WriteLine(ex);
            }

            return ReleaseManagerFlags.Unknown;
        }

        private static ReleaseManagerFlags Commit(IRepository repo, string version)
        {
            try
            {
                var sign = repo.Config.BuildSignature(DateTime.Now);
                repo.Commit($"chore(AppVersion): App version set to {version}.", sign, sign);

                return ReleaseManagerFlags.Ok;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return ReleaseManagerFlags.Unknown;
        }

        private static ReleaseManagerFlags Stage(IRepository repo)
        {
            try
            {
                var status = repo.RetrieveStatus();

                foreach (var file in status.Modified.Select(mods => mods.FilePath))
                {
                    repo.Index.Add(file);
                }

                repo.Index.Write();

                return ReleaseManagerFlags.Ok;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return ReleaseManagerFlags.Unknown;
        }

        private ReleaseManagerFlags Transaction(Func<Repository, ReleaseManagerFlags> func, ReleaseManagerFlags failFlag)
        {
            Repository repo = null;

            try
            {
                repo = new Repository(_rootDirectory);

                return func(repo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return failFlag;
            }
            finally
            {
                repo?.Dispose();
            }
        }

        private ReadOnlyDictionary<string, IPlatform> CreatePlatforms(IEnumerable<Platform> platforms)
        {
            var ps = new Dictionary<string, IPlatform>();

            foreach (var platform in platforms)
            {
                var absolutePath = Path.Combine(_rootDirectory, platform.Path);

                switch (platform.Name?.ToLower())
                {
                    case "dll":
                        ps.Add(platform.Name, new DLLPlatform(absolutePath));
                        break;
                    case "ios":
                        ps.Add(platform.Name, new IOSPlatform(absolutePath));
                        break;
                    case "droid":
                        ps.Add(platform.Name, new DroidPlatform(absolutePath));
                        break;
                    case "uwp":
                        ps.Add(platform.Name, new UWPPlatform(absolutePath));
                        break;
                    default:
                        ps.Add("null", new Invalid(absolutePath));
                        break;
                }
            }

            return new ReadOnlyDictionary<string, IPlatform>(ps);
        }

        private static bool TryParseFile(string filePath, out ConfigFile file)
        {
            file = default;
            try
            {
                file = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(filePath));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}