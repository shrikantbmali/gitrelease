using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using gitrelease.core.platforms;
using LibGit2Sharp;

namespace gitrelease.core
{
    public class ReleaseManager
    {
        private readonly string _rootDir;
        private Package _package;

        public ReleaseManager(string rootDir)
        {
            _rootDir = rootDir;
        }

        public ReleaseManagerFlags Initialize()
        {
            if (!Directory.Exists(_rootDir))
            {
                return ReleaseManagerFlags.InvalidDirectory;
            }

            if (_package != null)
            {
                return ReleaseManagerFlags.AlreadyInitialized;
            } 
            
            _package = new Package(_rootDir);
            
            return ReleaseManagerFlags.Ok;
        }

        public ReleaseManagerFlags Release(ReleaseChoices releaseChoices)
        {
            return ExecuteSafe(() =>
            {
                return ExecuteRepoSafe(repo =>
                {
                    var validityFlag = IsRepoReadyForRelease(repo);

                    if (validityFlag != ReleaseManagerFlags.Ok)
                        return validityFlag;

                    var config = _package.GetConfig();

                    if (config == null)
                        return ReleaseManagerFlags.InvalidConfigFile;

                    return Release(repo, config, releaseChoices);
                });
            });
        }

        private ReleaseManagerFlags Release(IRepository repo, ConfigFile configFile, ReleaseChoices releaseChoices)
        {
            var version = GetCurrentVersion(repo);

            version = InfuseCommitAndIncrement(repo, version, releaseChoices.ReleaseType);

            var releaseFlag = UpdatePackageVersion(version);

            if (releaseFlag != ReleaseManagerFlags.Ok)
                return releaseFlag;

            releaseFlag = UpdatePlatformVersions(version, configFile);

            if (releaseFlag != ReleaseManagerFlags.Ok)
                return releaseFlag;

            releaseFlag = CreateACommit(repo, version);

            if (releaseFlag != ReleaseManagerFlags.Ok)
                return releaseFlag;

            releaseFlag = UpdateChangelog();

            if (releaseFlag != ReleaseManagerFlags.Ok)
                return releaseFlag;

            releaseFlag = AmendLastCommit(repo, version);

            if (releaseFlag != ReleaseManagerFlags.Ok)
                return releaseFlag;

            releaseFlag = CreateTag(repo, version);

            return releaseFlag;
        }

        private static GitVersion InfuseCommitAndIncrement(IRepository repo, GitVersion gitVersion,
            ReleaseType releaseType)
        {
            var headTipSha = repo.Head.Tip.Id.Sha.Substring(0, 10);
            return new GitVersion(GetUpdateVersion(gitVersion, releaseType), headTipSha);
        }

        private static GitVersion GetUpdateVersion(GitVersion gitVersion, ReleaseType releaseType)
        {
            switch (releaseType)
            {
                case ReleaseType.Major:
                    return gitVersion.IncrementMajorAndGetNew();
                case ReleaseType.Minor:
                    return gitVersion.IncrementMinorAndGetNew();
                case ReleaseType.Patch:
                    return gitVersion.IncrementPatchAndGetNew();
                default:
                    throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null);
            }
        }

        private static ReleaseManagerFlags CreateACommit(IRepository repo, GitVersion version)
        {
            var result = Stage(repo);

            if (result != ReleaseManagerFlags.Ok)
                return result;

            try
            {
                var sign = repo.Config.BuildSignature(DateTime.Now);
                repo.Commit($"chore(VersionUpdate): {version.ToMajorMinorPatch()}", sign, sign);

                return ReleaseManagerFlags.Ok;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return ReleaseManagerFlags.Unknown;
        }

        private static ReleaseManagerFlags AmendLastCommit(IRepository repo, GitVersion version)
        {
            var result = Stage(repo);

            if (result != ReleaseManagerFlags.Ok)
                return result;

            try
            {
                var sign = repo.Config.BuildSignature(DateTime.Now);
                repo.Commit($"chore(VersionUpdate): {version.ToMajorMinorPatch()}", sign, sign, new CommitOptions()
                {
                    AmendPreviousCommit = true,
                });

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

        private static ReleaseManagerFlags CreateTag(IRepository repo, GitVersion version)
        {
            try
            {
                repo.ApplyTag($"v{version.ToMajorMinorPatch()}");
                return ReleaseManagerFlags.Ok;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return ReleaseManagerFlags.TagCreationFailed;
            }
        }

        private ReleaseManagerFlags UpdateChangelog()
        {
            try
            {
                var (_, isError) = CommandExecutor.ExecuteCommand("changelog", "generate", _rootDir);

                return isError ? ReleaseManagerFlags.ChangelogCreationFailed : ReleaseManagerFlags.Ok;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return ReleaseManagerFlags.ChangelogCreationFailed;
            }
        }

        private ReleaseManagerFlags UpdatePlatformVersions(GitVersion version, ConfigFile configFile)
        {
            if (configFile?.Platforms == null)
                return ReleaseManagerFlags.InvalidConfigFile;

            var platforms = CreatePlatforms(configFile.Platforms);

            if (platforms.Any(platform => platform.Value.Type == PlatformType.INVALID))
            {
                return ReleaseManagerFlags.InvalidConfigFile;
            }

            foreach (var platform in platforms)
            {
                var (flag, _) = platform.Value.Release(version);

                if (flag != ReleaseManagerFlags.Ok)
                    return flag;
            }

            return ReleaseManagerFlags.Ok;
        }

        private ReadOnlyDictionary<string, IPlatform> CreatePlatforms(IEnumerable<Platform> platforms)
        {
            var ps = new Dictionary<string, IPlatform>();

            foreach (var platform in platforms)
            {
                var absolutePath = Path.Combine(_rootDir, platform.Path);

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

        private ReleaseManagerFlags UpdateDllVersion(GitVersion version)
        {
            throw new NotImplementedException();
        }

        private ReleaseManagerFlags UpdatePackageVersion(GitVersion version)
        {
            return _package.SetVersion(version);
        }

        private GitVersion GetCurrentVersion(IRepository repo)
        {
            return _package.GetVersion();
        }

        private ReleaseManagerFlags IsRepoReadyForRelease(IRepository repo)
        {
            if (IsRepoDirty(repo))
            {
                return ReleaseManagerFlags.DirtyRepo;
            }

            if (!IsRepoInitialized(repo))
            {
                return ReleaseManagerFlags.RepoNotInitializedForReleaseProcess;
            }

            if (AreToolsAvailable(repo))
            {
                return ReleaseManagerFlags.InstallNpm;
            }

            return ReleaseManagerFlags.Ok;
        }

        private bool AreToolsAvailable(IRepository repo)
        {
            return _package.AreToolsAvailable();
        }

        private bool IsRepoInitialized(IRepository repo)
        {
            return _package.IsInitialized();
        }

        private static bool IsRepoDirty(IRepository repo)
        {
            return repo.RetrieveStatus(new StatusOptions())?.IsDirty ?? true;
        }

        private ReleaseManagerFlags ExecuteRepoSafe(Func<IRepository, ReleaseManagerFlags> func)
        {
            using var repo = new Repository(_rootDir);
            var repoHead = repo.Head;

            try
            {
                return func(repo);
            }
            catch (Exception e)
            {
                repo.Reset(ResetMode.Hard, repoHead.Tip);
                repo.RemoveUntrackedFiles();
                Console.WriteLine(e);
            }

            return ReleaseManagerFlags.Unknown;
        }

        private ReleaseManagerFlags ExecuteSafe(Func<ReleaseManagerFlags> func)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return ReleaseManagerFlags.Unknown;
        }

        public string[] GetVersion(string platformName)
        {
            return platformName == "all"
                ? CreatePlatforms(_package.GetConfig().Platforms).Select(p => p.Value.GetVersion()).ToArray()
                : new[] { CreatePlatforms(_package.GetConfig().Platforms)[platformName].GetVersion() };
        }
    }
}
