using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using gitrelease.core;

namespace gitrelease.cli
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var rootCommand = new RootCommand { Handler = CommandHandler.Create<string, ReleaseType>(ReleaseSequence) };
            rootCommand.AddOption(
                new Option<string>(
                    new [] { "--root" , "-r"},
                    Directory.GetCurrentDirectory,
                    "Specify the root folder if the current executing directory is not a intended folder"));

            rootCommand.AddOption(new Option<ReleaseType>(
                new[] {"--bump", "-b"},
                "Specifies whether to update Major, Minor or Patch version")
            {
                IsRequired = true
            });

            var getVersionCommand = new Command("get-version", "Gets the current versions used by repositories.")
            {
                Handler = CommandHandler.Create<string, string>(GetVersion),
            };

            getVersionCommand.AddAlias("gv");

            getVersionCommand.AddOption(
                new Option<string>(
                    new [] { "--platform", "-p"},
                    () => "all",
                    "Gets version for specific platform."));

            getVersionCommand.AddOption(
                new Option<string>(
                    new [] { "--root", "-r"},
                    Directory.GetCurrentDirectory,
                    "Specify the root folder if the current executing directory is not a intended folder"));

            rootCommand.AddCommand(getVersionCommand);

            rootCommand.Parse(args);

            return rootCommand.Invoke(args);
        }

        private static int ReleaseSequence(string root, ReleaseType bump)
        {
            var manager = new ReleaseManager(root == "." ? Directory.GetCurrentDirectory() : root);

            manager.Initialize();

            var releaseManagerFlags = manager.Release(new ReleaseChoices()
            {
                ReleaseType = bump
            });

            DumpMessage(releaseManagerFlags);

            return (int)releaseManagerFlags;
        }

        private static int GetVersion(string root, string platform = "all")
        {
            var manager = new ReleaseManager(root);

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
}
