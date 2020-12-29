using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading;
using gitrelease.core;

namespace gitrelease.cli
{
    class Program
    {
        static int Main(string[] args)
        {
            var rootCommand = new RootCommand { Handler = CommandHandler.Create(ReleaseSequence) };

            var getVersionCommand = new Command("get-version", "Gets the current versions used by repositories.")
            {
                Handler = CommandHandler.Create<string>(GetVersion),
            };

            getVersionCommand.AddOption(new Option<string>("--platform", () => "all", "Gets version for specific platform."));

            getVersionCommand.AddAlias("gv");
            rootCommand.AddCommand(getVersionCommand);

            var initCommand = new Command("init", "Initialized the repository for the release command.")
            {
                Handler = CommandHandler.Create(() => Init())
            };
            
            rootCommand.AddCommand(initCommand);

            rootCommand.Parse(args);

            return rootCommand.Invoke(args);
        }

        private static int GetVersion(string platformName = "all")
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
                    string[] versions = releaseManager.GetVersion(platformName);

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

        private static int ReleaseSequence()
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
                    releaseManagerFlag = releaseManager.Release();
                }
            }
            else
            {
                Console.WriteLine("No config file found or it is invalid, use release init command to generate a config file");
            }

            return (int)releaseManagerFlag;
        }

        private static ReleaseManagerFlags Init()
        {
            try
            {
                var initer = Builder.New()
                    .Initer()
                    .UseRoot(Directory.GetCurrentDirectory())
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

                return initer.Init();
            }
            catch (OperationCanceledException)
            {
                return ReleaseManagerFlags.Cancelled;
            }
        }
    }
}
