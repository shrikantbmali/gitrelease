#nullable enable
using System;

namespace gitrelease.core
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

        public IIniterBuilder Initer()
        {
            return new IniterBuilder();
        }
    }

    public interface IBuilder
    {
        IBuilder UseRoot(string configFilePath);

        (BuilderFlags Flag, IReleaseManager ReleaseManager) Create();

        IIniterBuilder Initer();
    }

    public interface IIniterBuilder
    {
        IIniterBuilder GetPlatform(Func<string, string?> func);

        IIniterBuilder GetPlatformPath(Func<string, string?> func);
        
        IIniterBuilder UseRoot(string root);

        IIniter Create();
    }

    internal class IniterBuilder : IIniterBuilder
    {
        private Func<string, string?> platformGetter;
        private Func<string, string?> pathgetter;
        private string root;

        public IIniterBuilder GetPlatform(Func<string, string?> func)
        {
            this.platformGetter = func;
            return this;
        }

        public IIniterBuilder GetPlatformPath(Func<string, string?> func)
        {
            this.pathgetter = func;
            return this;
        }

        public IIniterBuilder UseRoot(string root)
        {
            this.root = root;
            return this;
        }

        public IIniter Create()
        {
            return new Initer(this.root, this.platformGetter, pathgetter);
        }
    }
}
