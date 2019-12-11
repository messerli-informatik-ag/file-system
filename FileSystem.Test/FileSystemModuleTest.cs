using System;
using Autofac;
using Messerli.CompositionRoot;
using Messerli.Test.Utility;
using Xunit;

namespace Messerli.FileSystem.Test
{
    public sealed class FileSystemModuleTest : IDisposable
    {
        private readonly IContainer _container =
            new CompositionRootBuilder()
                .RegisterModule(new FileSystemModule())
                .Build();

        [Theory]
        [ClassData(typeof(ObjectArrayEnumerable<ModuleInterfaceEnumerable<FileSystemModule>>))]
        public void ModuleCanBeResolved(Type type)
        {
            Assert.NotNull(_container.Resolve(type));
        }

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}
