using System.Collections.Generic;

namespace Messerli.FileSystem
{
    public interface IFileSystem
    {
        /// <summary>
        /// Checks if the given <paramref name="path" /> exists.
        /// It may be either a file or a directory.
        /// </summary>
        bool Exists(string path);

        bool ExistsAndIsDirectory(string path);

        bool ExistsAndIsFile(string path);

        /// <summary>
        /// Deletes a file or directory. Directories are deleted recursively.
        /// </summary>
        /// <exception cref="System.IO.IOException">If an underlying call to a method in <see cref="System.IO"/> throws.</exception>
        void Delete(string path);

        /// <summary>
        /// Moves a file or directory.
        /// </summary>
        /// <exception cref="System.IO.IOException">If an underlying call to a method in <see cref="System.IO"/> throws.</exception>
        /// <exception cref="System.IO.IOException">If the <paramref name="destination"/> already exists.</exception>
        void Move(string source, string destination);

        /// <summary>
        /// Copies a file or directory. Directories are copied recursively.
        /// </summary>
        /// <exception cref="System.IO.IOException">If an underlying call to a method in <see cref="System.IO"/> throws.</exception>
        /// <exception cref="System.IO.IOException">If the <paramref name="destination"/> already exists.</exception>
        void Copy(string source, string destination);

        /// <summary>
        /// Creates all directories in <paramref name="path"/> unless they already exist.
        /// </summary>
        /// <exception cref="System.IO.IOException">If an underlying call to a method in <see cref="System.IO"/> throws.</exception>
        void CreateDirectory(string path);

        /// <summary>
        /// Returns the names of subdirectories (including their paths) in the specified directory.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is case-sensitive.</param>
        /// <returns>An IEnumerable of the full names (including paths) of subdirectories in the specified path, or an empty array if no directories are found.</returns>
        IEnumerable<string> GetFiles(string path);

        /// <summary>
        /// Returns the names of files(including their paths) that match the specified search pattern in the specified directory.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is case-sensitive.</param>
        /// <param name="searchPattern">The search string to match against the names of files in path. This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
        /// <returns>An IEnumerable of the full names (including paths) for the files in the specified directory.</returns>
        IEnumerable<string> GetFiles(string path, string searchPattern);

        /// <param name="path">The relative or absolute path to the directory to search. This string is case-sensitive.</param>
        bool IsDirectoryWritable(string path);
    }
}
