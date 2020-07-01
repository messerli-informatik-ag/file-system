using System.IO;

namespace Messerli.FileSystem
{
    public interface IFileOpeningBuilder
    {
        /// <summary>
        /// Specifies that <see cref="Open"/> should create a new file. If the file already exists, it will be overwritten.
        /// </summary>
        IFileOpeningBuilder Create(bool create = true);

        /// <summary>
        /// Specifies that <see cref="Open"/> should open an existing file. When the file is opened, it should be truncated so that its size is zero bytes.
        /// Requires that <see cref="Write"/> is called too. Can not be used to together with <see cref="Append"/>.
        /// </summary>
        IFileOpeningBuilder Truncate(bool truncate = true);

        /// <summary>
        /// Specifies that <see cref="Open"/> should open the file if it exists and seek to the end of it, or create a new file.
        /// Can not be used to together with <see cref="Truncate"/>.
        /// </summary>
        IFileOpeningBuilder Append(bool append = true);

        /// <summary>
        /// Specifies that <see cref="Open"/> should open the file with write access.
        /// Data can be written to the file. Combine with <see cref="Read"/> for read/write access.
        /// </summary>
        IFileOpeningBuilder Write(bool write = true);

        /// <summary>
        /// Specifies that <see cref="Open"/> should open the file with read access.
        /// Combine with <see cref="Write"/> for read/write access.
        /// </summary>
        IFileOpeningBuilder Read(bool read = true);

        /// <summary>
        /// Specifies that <see cref="Open"/> should create a new file.
        /// If the file already exists, an <see cref="IOException"/> exception is thrown.
        /// </summary>
        IFileOpeningBuilder CreateNew(bool createNew = true);

        /// <exception cref="IOException" />
        Stream Open(string path);
    }
}
