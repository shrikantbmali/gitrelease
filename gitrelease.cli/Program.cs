using gitrelease;
using System;
using System.IO;

namespace release
{
    class Program
    {
        static void Main(string[] args)
        {
            var build = Builder.New()
                            .UseRoot(Directory.GetCurrentDirectory())
                            .Create();

            if (build.Flag == BuilderFlags.Ok)
            {
                ReleaseManagerFlags releaseManagerFlag = build.ReleaseManager.Initialize();

                if (releaseManagerFlag == ReleaseManagerFlags.Ok)
                {
                    ReleaseSequenceFlags result = build.ReleaseManager.Release();
                }
            }
            else
            {
                Console.WriteLine("No config file found or it is invalid, use release init command to generate a config file");
            }
        }
    }
}
