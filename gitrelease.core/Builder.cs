namespace gitrelease
{
    public class Builder : IBuilder
    {
        private string root;

        private Builder() { }

        public static IBuilder New()
        {
            return new Builder();
        }

        public IBuilder UseRoot(string root)
        {
            this.root = root;

            return this;
        }

        public (BuilderFlags Flag, IReleaseManager ReleaseManager) Create()
        {
            return (Flag: BuilderFlags.Ok, ReleaseManager: new ReleaseManager(this.root));
        }
    }

    public interface IBuilder
    {
        IBuilder UseRoot(string configFilePath);

        (BuilderFlags Flag, IReleaseManager ReleaseManager) Create();
    }
}
