using System.IO;

namespace Messerli.FileSystem
{
    public sealed class FileOpeningBuilderStub : IFileOpeningBuilder
    {
        public IFileOpeningBuilder Create(bool create = true) => this;

        public IFileOpeningBuilder Truncate(bool truncate = true) => this;

        public IFileOpeningBuilder Append(bool append = true) => this;

        public IFileOpeningBuilder Write(bool write = true) => this;

        public IFileOpeningBuilder Read(bool read = true) => this;

        public IFileOpeningBuilder CreateNew(bool createNew = true) => this;

        public Stream Open(string path) => new MemoryStream();
    }
}
