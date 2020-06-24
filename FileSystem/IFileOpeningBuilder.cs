using System.IO;

namespace Messerli.FileSystem
{
    public interface IFileOpeningBuilder
    {
        /// <summary>
        /// Specifies that the operating system should create a new file. If the file already exists, it will be overwritten.
        /// </summary>
        IFileOpeningBuilder Create(bool create = true);

        /// <summary>
        /// Opens the file if it exists and seeks to the end of the file, or creates a new file.
        /// Requires that <see cref="Write"/> is called to.
        /// </summary>
        IFileOpeningBuilder Truncate(bool truncate = true);

        /// <summary>
        /// Opens the file if it exists and seeks to the end of the file, or creates a new file.
        /// </summary>
        IFileOpeningBuilder Append(bool append = true);

        /// <summary>
        /// Write access to the file. Data can be written to the file. Combine with <see cref="Read"/> for read/write access.
        /// </summary>
        IFileOpeningBuilder Write(bool write = true);

        /// <summary>
        /// Read access to the file. Data can be read from the file. Combine with <see cref="Write"/> for read/write access.
        /// </summary>
        IFileOpeningBuilder Read(bool read = true);

        /// <summary>
        /// Specifies that the operating system should create a new file.
        /// If the file already exists, an <see cref="IOException"/> exception is thrown.
        /// </summary>
        IFileOpeningBuilder CreateNew(bool createNew = true);

        /// <exception cref="IOException" />
        Stream Open(string path);
    }
}
