using System;
using System.IO;
using System.Reflection;

namespace release
{
    class Program
    {
        static void Main(string[] args)
        {
            var rootPath = Directory.GetCurrentDirectory();

            Console.WriteLine(rootPath);
            if (!ConfigFileExists(rootPath, out var configFilePath))
            {
                Console.WriteLine("gitrelease config file could not be found!", ConsoleColor.Red);
            }

            var releaseManager = gitrelease
                                .Builder
                                .New()
                                .UseConfig(configFilePath)
                                .Create();
        }

        private static bool ConfigFileExists(string rootPath, out string filePath)
        {
            filePath = Path.Combine(rootPath, gitrelease.ConfigFile.FixName);
            return File.Exists(filePath);
        }
    }
}
