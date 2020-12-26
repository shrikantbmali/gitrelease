namespace gitrelease
{
    internal class ReleaseManager : IReleaseManager
    {
        private ConfigFile? file = null;

        public ReleaseManager()
        {
            this.file = null;
        }

        public ReleaseManager(ConfigFile file)
        {
            this.file = file;
        }
    }
}