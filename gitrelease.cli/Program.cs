using gitrelease;
using System;

namespace release
{
    class Program
    {
        static void Main(string[] _)
        {
            var build = Builder.New()
                            .UseRoot(
                //Directory.GetCurrentDirectory()
                @"C:\Users\ShrikantMali\sandbox\ac\main"
                )
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
