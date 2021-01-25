using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Drawing;
using System.IO;
using System.Linq;
using gitrelease.core;

namespace gitrelease.cli
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                Handler = CommandHandler.Create<string, ReleaseType, string, string, bool, bool, bool, bool, uint, string, ChangeLogType>(ReleaseSequence)
            };

            rootCommand.AddOption(
                new Option<string>(
                    new[] { "--root", "-r" },
                    ParseRoot,
                    true,
                    "Specify the root folder if the current executing directory is not a intended folder"));

            rootCommand.AddOption(
                new Option<bool>(
                    new[] { "--dry-run", "-d" },
                    () => false,
                    "Runs a command dry without crating a tag and a commit."));
            rootCommand.AddOption(
                new Option<bool>(
                    new[] { "--ignore-dirty", "-i" },
                    () => false,
                    "Runs a command dry without crating a tag and a commit."));
            rootCommand.AddOption(
                new Option<bool>(
                    new[] { "--skip-changelog", "-c" },
                    () => false,
                    "Specify if changelog creation should be skipped."));
            rootCommand.AddOption(
                new Option<uint>(
                    new[] { "--changelog-character-limit", "-l" },
                    () => 0,
                    "Specify the limit of characters limit for changelog file (strictly implemented for azure dev-ops limit of 5000 characters)."));
            rootCommand.AddOption(
                new Option<string>(
                    new[] { "--changelog-filename", "-f" },
                    () => "CHANGELOG.md",
                    "Specify the changelog filename."));
            rootCommand.AddOption(
                new Option<ChangeLogType>(
                    new[] { "--changelog-type", "-h" },
                    () => ChangeLogType.All,
                    "Specify the changelog type."));
            rootCommand.AddOption(
                new Option<bool>(
                    new[] { "--skip-tag", "-g" },
                    () => false,
                    "Specify if tag creation should skipped"));
            rootCommand.AddOption(
                new Option<string>(
                    new[] { "--pre-release-tag", "-p" },
                    "Any pre release tap you'd like to add to the version."));
            rootCommand.AddArgument(
                new Argument<ReleaseType>(
                    "release-type",
                    "Specify the release type")
                {
                    Arity = ArgumentArity.ExactlyOne
                });

            rootCommand.AddArgument(
                new Argument<string>(
                    "version",
                    "Specify the release version")
                {
                    Arity = ArgumentArity.ZeroOrOne
                });

            var getVersionCommand = new Command(
                "get-version",
                "Gets the current versions used by repositories.")
            {
                Handler = CommandHandler.Create<string, string>(GetVersion),
            };

            getVersionCommand.AddAlias("gv");

            getVersionCommand.AddOption(
                new Option<string>(
                    new[] { "--platform", "-t" },
                    () => "all",
                    "Gets version for specific platform."));

            getVersionCommand.AddOption(
                new Option<string>(
                    new[] { "--root", "-r" },
                    ParseRoot,
                    true,
                    "Specify the root folder if the current executing directory is not a intended folder"));

            rootCommand.AddCommand(getVersionCommand);


            var initCommand = new Command("init", "Initialized the repo for git-release workflow")
            {
                Handler = CommandHandler.Create<string, bool>(Init)
            };

            initCommand.AddOption(new Option<string>(
                new[] { "--root", "-r" },
                ParseRoot,
                true,
                "Specify the root folder if the current executing directory is not a intended folder"));

            initCommand.AddOption(new Option<bool>(
                new[] { "--native", "-n" },
                () => false,
                "Select in case the project is not a xamarin project."));

            rootCommand.AddCommand(initCommand);

            rootCommand.Parse(args);

            return rootCommand.Invoke(args);
        }

        private static string ParseRoot(ArgumentResult result)
        {
            var pathValue = result.Tokens.FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(pathValue))
            {
                return Directory.GetCurrentDirectory();
            }

            return Path.IsPathRooted(pathValue) ? pathValue : Path.Combine(Directory.GetCurrentDirectory(), pathValue);
        }

        private static int Init(string root, bool native)
        {
            var releaseManager = new ReleaseManager(root, CreateMessenger());

            var result = releaseManager.Initialize();

            if (result != ReleaseManagerFlags.Ok)
            {
                DumpMessage(result);
                return (int)result;
            }

            result = releaseManager.SetupRepo(native);

            DumpMessage(result);
            return (int)result;
        }

        private static IMessenger CreateMessenger()
        {
            return new ConsoleMessenger();
        }

        private static void ReleaseSequence(
            string root,
            ReleaseType releaseType,
            string version,
            string preReleaseTag,
            bool dryRun,
            bool skipTag,
            bool skipChangelog,
            bool ignoreDirty,
            uint changelogCharacterLimit,
            string changelogFileName,
            ChangeLogType changeLogType)
        {
            root = FindDirectory(root);

            if (releaseType == ReleaseType.Custom && !GitVersion.IsValid(version))
            {
                Console.WriteLine(
                    "when selecting custom version type, version must be provided in format {Major}.{Minor}.{Patch}");

                return;
            }

            var manager = new ReleaseManager(root == "." ? Directory.GetCurrentDirectory() : root, CreateMessenger());

            manager.Initialize();

            var releaseManagerFlags = manager.Release(new ReleaseChoices
            {
                ReleaseType = releaseType,
                CustomVersion = releaseType == ReleaseType.Custom
                    ? GitVersion.Parse(version, preReleaseTag)
                    : string.IsNullOrEmpty(preReleaseTag)
                        ? GitVersion.Empty
                        : GitVersion.GetPrerelease(preReleaseTag),
                DryRun = dryRun,
                SkipTag = skipTag,
                SkipChangelog = skipChangelog,
                IgnoreDirty = ignoreDirty,
                ChangelogCharacterLimit = changelogCharacterLimit,
                ChangelogFileName = changelogFileName,
                ChangeLogType = changeLogType
            });

            DumpMessage(releaseManagerFlags);

            return;// (int)releaseManagerFlags;
        }

        private static string FindDirectory(string root)
        {
            try
            {
                while (true)
                {
                    var configFile = Directory.GetFiles(root, ConfigFileName.FixName);
                    if (configFile.Any())
                    {
                        return root;
                    }

                    root = Path.GetDirectoryName(root);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return root;
            }
        }

        private static int GetVersion(string root, string platform)
        {
            var manager = new ReleaseManager(root, CreateMessenger());

            manager.Initialize();

            var versions = manager.GetVersion(platform);

            foreach (var version in versions)
            {
                Console.WriteLine(version);
            }

            return 0;
        }

        private static void DumpMessage(ReleaseManagerFlags releaseManagerFlag)
        {
            Console.WriteLine(releaseManagerFlag.ToString());
        }
    }

    internal class ConsoleMessenger : IMessenger
    {
        public void Info(string message)
        {
            Colorful.Console.WriteLineFormatted(message, Color.Yellow);
        }

        public void Error(Exception exception)
        {
            Colorful.Console.WriteLineFormatted(exception?.ToString(), Color.Red);
        }
    }
}
