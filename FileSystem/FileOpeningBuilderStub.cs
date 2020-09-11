using System;
using System.IO;

namespace Messerli.FileSystem
{
    public sealed class FileOpeningBuilderStub : IFileOpeningBuilder
    {
        private readonly Func<Stream> _createStream;

        public FileOpeningBuilderStub()
            : this(CreateMemoryStream)
        {
        }

        public FileOpeningBuilderStub(Func<Stream> createStream)
            => _createStream = createStream;

        public IFileOpeningBuilder Create(bool create = true) => this;

        public IFileOpeningBuilder Truncate(bool truncate = true) => this;

        public IFileOpeningBuilder Append(bool append = true) => this;

        public IFileOpeningBuilder Write(bool write = true) => this;

        public IFileOpeningBuilder Read(bool read = true) => this;

        public IFileOpeningBuilder CreateNew(bool createNew = true) => this;

        public Stream Open(string path) => _createStream();

        private static Stream CreateMemoryStream() => new MemoryStream();
    }
}
