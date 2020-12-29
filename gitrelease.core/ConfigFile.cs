using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gitrelease.core
{
    internal struct ConfigFile
    {
        [JsonPropertyName("platforms")]
        public IEnumerable<Platform> Platforms { get; set; }

        [JsonIgnore]
        public const string FixName = "gitrelease.config.json";
    }

    internal struct Platform
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }
    }

    internal static class Config
    {
        public static bool Save(this ConfigFile file, string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                JsonSerializer.Serialize(
                    new Utf8JsonWriter(new FileStream(filePath, FileMode.CreateNew)),
                    file, options);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}