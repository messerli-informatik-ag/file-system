using System.IO;

namespace Messerli.FileSystem
{
    public interface IFileOpeningBuilder
    {
        IFileOpeningBuilder Create(bool create = true);

        IFileOpeningBuilder Truncate(bool truncate = true);

        IFileOpeningBuilder Append(bool append = true);

        IFileOpeningBuilder Write(bool write = true);

        IFileOpeningBuilder Read(bool read = true);

        IFileOpeningBuilder CreateNew(bool createNew = true);

        Stream Open(string path);
    }
}
