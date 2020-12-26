using System;
using System.IO;
using System.Text.Json;

namespace gitrelease
{
    public class Builder : IBuilder
    {
        private bool usingConfig;
        private string configFilePath;

        private Builder() { }

        public static IBuilder New()
        {
            return new Builder();
        }

        public IBuilder UseConfig(string configFilePath)
        {
            this.usingConfig = true;
            this.configFilePath = configFilePath;

            return this;
        }

        public (BuilderFlags Flag, IReleaseManager ReleaseManager) Create()
        {
            ConfigFile configFile = default;

            if (this.usingConfig && !TryParseFile(out configFile, this.configFilePath))
            {
                return (BuilderFlags.InvalidFile, null);
            }

            return (
                Flag: BuilderFlags.Ok,
                ReleaseManager: this.usingConfig
                    ? new ReleaseManager(configFile)
                    : new ReleaseManager()
                );
        }

        private static bool TryParseFile(out ConfigFile file, string filePath)
        {
            file = default;
            try
            {
                file = JsonSerializer.Deserialize<ConfigFile>(File.ReadAllText(filePath));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }

    public interface IBuilder
    {
        IBuilder UseConfig(string configFilePath);

        (BuilderFlags Flag, IReleaseManager ReleaseManager) Create();
    }
}
