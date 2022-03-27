using System;
using System.CommandLine;
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
            var rootOption = new Option<string>(new[] { "--root", "-r" }, ParseRoot, true, "Specify the root folder if the current executing directory is not a intended folder");
            var dryRunOption = new Option<bool>(new[] { "--dry-run", "-d" }, () => false, "Runs a command dry without crating a tag and a commit.");
            var ignoreDirtyOption = new Option<bool>(new[] { "--ignore-dirty", "-i" }, () => false, "Runs a command dry without crating a tag and a commit.");
            var skipChangelogOption = new Option<bool>(new[] { "--skip-changelog", "-c" }, () => false, "Specify if changelog creation should be skipped.");
            var changelogCharacterLimitOption = new Option<uint>(new[] { "--changelog-character-limit" }, () => 0, "Specify the limit of characters limit for changelog file (strictly implemented for azure dev-ops limit of 5000 characters).");
            var changelogFilenameOption = new Option<string>(new[] { "--changelog-filename" }, () => "CHANGELOG.md", "Specify the changelog filename.");
            var changelogTypeOption = new Option<ChangeLogType>(new[] { "--changelog-type" }, () => ChangeLogType.Incremental, "Specify the changelog type.");
            var skipTagOption = new Option<bool>(new[] { "--skip-tag", "-t" }, () => false, "Specify if tag creation should skipped");
            var preReleaseOption = new Option<string>(new[] { "--pre-release-tag", "-p" }, "Any pre release tap you'd like to add to the version.");
            var appendStringOption = new Option<string>(new[] { "--append-string" }, () => string.Empty, "Specify the string you'd like to append at the end. It will only be appended in Incremental changelog type.");
            var releaseTypeArgument = new Argument<ReleaseType>("release-type", "Specify the release type") {Arity = ArgumentArity.ExactlyOne};
            var versionArgument = new Argument<string>("version", "Specify the release version") {Arity = ArgumentArity.ZeroOrOne};
            var versionOption = new Option<string>(new[] { "--version", "-v" }, () => GitVersion.InitDefault.ToVersionString(), "Select in case the project is not a xamarin project.");
            var platformOption = new Option<string>(new[] { "--platform", "-t" }, () => "package", "Gets version for specific platform.");

            var rootCommand = new RootCommand();
            rootCommand.SetHandler((
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
                ChangeLogType changelogType,
                string appendString) => ReleaseSequence(
                    root,
                    releaseType,
                    version,
                    preReleaseTag,
                    dryRun,
                    skipTag,
                    skipChangelog,
                    ignoreDirty,
                    changelogCharacterLimit,
                    changelogFileName,
                    changelogType,
                    appendString),
                    rootOption,
                    releaseTypeArgument,
                    versionArgument,
                    preReleaseOption,
                    dryRunOption,
                    skipTagOption,
                    skipChangelogOption,
                    ignoreDirtyOption,
                    changelogCharacterLimitOption,
                    changelogFilenameOption,
                    changelogTypeOption,
                    appendStringOption);


            rootCommand.AddOption(rootOption);
            rootCommand.AddOption(dryRunOption);
            rootCommand.AddOption(ignoreDirtyOption);
            rootCommand.AddOption(skipChangelogOption);
            rootCommand.AddOption(changelogCharacterLimitOption);
            rootCommand.AddOption(changelogFilenameOption);
            rootCommand.AddOption(changelogTypeOption);
            rootCommand.AddOption(skipTagOption);
            rootCommand.AddOption(preReleaseOption);
            rootCommand.AddOption(appendStringOption);
            rootCommand.AddArgument(releaseTypeArgument);
            rootCommand.AddArgument(versionArgument);
            
            var getVersionCommand = new Command("get-version", "Gets the current versions used by repositories.");
            getVersionCommand.SetHandler((string root, string platform) => GetVersion(root, platform), rootOption, platformOption);
            getVersionCommand.AddAlias("gv");
            getVersionCommand.AddOption(platformOption);
            getVersionCommand.AddOption(rootOption);

            rootCommand.AddCommand(getVersionCommand);

            var initCommand = new Command("init", "Initialized the repo for git-release workflow");
            initCommand.SetHandler((string root, string version) => Init(root, version), rootOption, versionOption);
            initCommand.AddOption(rootOption);
            initCommand.AddOption(versionOption);

            rootCommand.AddCommand(initCommand);

            rootCommand.Parse(args);

            return rootCommand.Invoke(args);
        }

        private static int ReleaseSequence(
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
            ChangeLogType changelogType,
            string appendString)
        {
            root = FindDirectory(root);
            if (string.IsNullOrEmpty(root))
            {
                new ConsoleMessenger().Error(
                    new FileNotFoundException("Invalid directory, provided directory or any of it's parents do not have a version.config file."));
            }

            if (releaseType == ReleaseType.Custom && !GitVersion.IsValid(version))
            {
                Console.WriteLine(
                    "when selecting custom version type, version must be provided in format {Major}.{Minor}.{Patch}");

                return -1;
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
                ChangeLogType = changelogType,
                AppendValue = appendString
            });

            DumpMessage(releaseManagerFlags);

            return (int)releaseManagerFlags;
        }

        private static string ParseRoot(ArgumentResult result)
        {
            var pathValue = result.Tokens.FirstOrDefault().Value;
            
            if (string.IsNullOrEmpty(pathValue))
            {
                return Directory.GetCurrentDirectory();
            }

            return Path.IsPathRooted(pathValue) ? pathValue : Path.Combine(Directory.GetCurrentDirectory(), pathValue);
        }

        private static int Init(string root, string version)
        {
            var releaseManager = new ReleaseManager(root, CreateMessenger());

            var result = releaseManager.Initialize();

            if (result != ReleaseManagerFlags.Ok)
            {
                DumpMessage(result);
                return (int)result;
            }

            result = releaseManager.SetupRepo(GitVersion.Parse(version));

            DumpMessage(result);
            return (int)result;
        }

        private static IMessenger CreateMessenger()
        {
            return new ConsoleMessenger();
        }

        private static string FindDirectory(string root)
        {
            try
            {
                while (root != null)
                {
                    var configFile = Directory.GetFiles(root, ConfigFileName.FixName);

                    if (configFile.Any())
                    {
                        return root;
                    }

                    root = Path.GetDirectoryName(root);
                } 

                return null;
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

        public string AskUser(string message, Func<string, bool> validate)
        {
            while (true)
            {
                Info(message);

                var input = Console.ReadLine();

                var valid = validate(input);

                if (valid)
                    return input;
            }
        }
    }
}
