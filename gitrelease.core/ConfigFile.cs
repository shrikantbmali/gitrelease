using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace gitrelease.core
{
    internal struct ConfigFile
    {
        [JsonProperty("platforms")]
        public IEnumerable<Platform> Platforms { get; set; }

        [JsonIgnore]
        public const string FixName = "gitrelease.config.json";
    }

    internal struct Platform
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }

    internal static class Config
    {
        public static bool Save(this ConfigFile file, string filePath)
        {
            try
            {
                var js = JsonConvert.SerializeObject(file, Formatting.Indented);
                File.WriteAllText(filePath, js);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            return true;
        }
    }
}