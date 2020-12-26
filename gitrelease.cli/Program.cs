using gitrelease;
using System;
using System.IO;

namespace release
{
    class Program
    {
        static void Main(string[] _)
        {
            var rootPath = Directory.GetCurrentDirectory();

            if (!ConfigFileExists(rootPath, out var configFilePath))
            {
                Console.WriteLine("gitrelease config file could not be found!", ConsoleColor.Red);
            }

            var releaseManager =
                                Builder
                                .New()
                                .UseConfig(configFilePath)
                                .Create();
            
            if(releaseManager.Flag == BuilderFlags.Ok)
            {
                ReleaseManagerFlags releaseManagerFlag = releaseManager.ReleaseManager.Initialize();

                if(releaseManagerFlag == ReleaseManagerFlags.Ok)
                {
                    ReleaseSequenceFlags result = releaseManager.ReleaseManager.Release();
                }
            }
            else
            {
                Console.WriteLine("No config file found or it is invalid, use release init command to generate a config file");
            }
        }

        private static bool ConfigFileExists(string rootPath, out string filePath)
        {
            filePath = Path.Combine(rootPath, ConfigFile.FixName);
            return File.Exists(filePath);
        }
    }
}
