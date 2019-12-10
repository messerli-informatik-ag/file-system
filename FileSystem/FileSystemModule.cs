using Autofac;

namespace Messerli.FileSystem
{
    public sealed class FileSystemModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FileSystem>().As<IFileSystem>();
        }
    }
}
