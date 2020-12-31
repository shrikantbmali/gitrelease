using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using gitrelease.core;

namespace gitrelease.cli
{
    internal static class Program
    {
        static int Main(string[] args)
        {
            var rootCommand = new RootCommand { Handler = CommandHandler.Create<string>(ReleaseSequence) };
            rootCommand.AddOption(new Option<string>(new []{ "--root" , "-r"}, () => ".", "Specify the root folder if the current executing directory is not a intended folder"));

            var getVersionCommand = new Command("get-version", "Gets the current versions used by repositories.")
            {
                Handler = CommandHandler.Create<string>(GetVersion),
            };

            getVersionCommand.AddOption(new Option<string>("--platform", () => "all", "Gets version for specific platform."));
            getVersionCommand.AddAlias("gv");

            rootCommand.AddCommand(getVersionCommand);

            var initCommand = new Command("init", "Initialized the repository for the release command.")
            {
                Handler = CommandHandler.Create<string>(Init)
            };

            initCommand.AddOption(
                new Option<string>(new[] {"--root", "-r"}, () => ".",
                    "Specify the root folder if the current executing directory is not a intended folder")
                );
            
            rootCommand.AddCommand(initCommand);

            rootCommand.Parse(args);

            return rootCommand.Invoke(args);
        }

        private static int ReleaseSequence(string root)
        {
            var (flag, releaseManager) = Builder.New()
                .UseRoot(root == "." ? Directory.GetCurrentDirectory() : root)
                .Create();

            var releaseManagerFlag = ReleaseManagerFlags.Unknown;

            if (flag == BuilderFlags.Ok)
            {
                releaseManagerFlag = releaseManager.Initialize();

                if (releaseManagerFlag == ReleaseManagerFlags.Ok)
                {
                    releaseManagerFlag = releaseManager.Release();


                    DumpMessage(releaseManagerFlag);
                }
            }
            else
            {
                Console.WriteLine("No config file found or it is invalid, use release init command to generate a config file");
            }

            return (int)releaseManagerFlag;
        }

        private static int GetVersion(string platform = "all")
        {
            var (flag, releaseManager) = Builder.New()
                .UseRoot(Directory.GetCurrentDirectory())
                .Create();

            var releaseManagerFlag = ReleaseManagerFlags.Unknown;

            if (flag == BuilderFlags.Ok)
            {
                releaseManagerFlag = releaseManager.Initialize();

                if (releaseManagerFlag == ReleaseManagerFlags.Ok)
                {
                    var versions = releaseManager.GetVersion(platform);

                    foreach (var version in versions)
                    {
                        Console.WriteLine(version);
                    }
                }
            }
            else
            {
                Console.WriteLine("No config file found or it is invalid, use release init command to generate a config file");
            }

            return (int)releaseManagerFlag;
        }

        private static int Init(string root)
        {
            try
            {
                var initer = Builder.New()
                    .Initer()
                    .UseRoot(root == "." ? Directory.GetCurrentDirectory() : root)
                    .GetPlatform(p =>
                    {
                        Console.WriteLine(p);
                        return Console.ReadLine();
                    })
                    .GetPlatformPath(p =>
                    {
                        Console.WriteLine(p);
                        return Console.ReadLine();
                    })
                    .Create();

                return (int)initer.Init();
            }
            catch (OperationCanceledException)
            {
                return (int)ReleaseManagerFlags.Cancelled;
            }
        }


        private static void DumpMessage(ReleaseManagerFlags releaseManagerFlag)
        {
            Console.WriteLine(releaseManagerFlag.ToString());
        }
    }
}
