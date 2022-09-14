using System;
using System.IO;
using System.Linq;
using Funcky;
using Funcky.Extensions;
using Funcky.Monads;

namespace Messerli.FileSystem
{
    public sealed class HierarchyLocator : IHierarchyLocator
    {
        private readonly IFileSystem _fileSystem;

        public HierarchyLocator()
            : this(new FileSystem())
        {
        }

        public HierarchyLocator(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public Option<string> FindClosestParentDirectoryContainingFile(string fileName, string startingDirectory)
        {
            if (Path.GetInvalidFileNameChars().Any(fileName.Contains))
            {
                throw new ArgumentException("File name contains invalid characters", nameof(fileName));
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name should not be empty", nameof(fileName));
            }

            return FindFirstDirectoryContainingFileWithCanonicalizedPath(fileName, CanonicalizePath(startingDirectory));
        }

        // We trim trailing directory separator chars because Path.GetDirectoryName
        // doesn't return the real parent directory for paths with trailing directory separators
        // it simply pops off the last separator. This doesn't break our search algorithm, but
        // results in unnecessary File.Exists checks.
        private static string CanonicalizePath(string path)
            => Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);

        private Option<string> FindFirstDirectoryContainingFileWithCanonicalizedPath(string fileName, string startingDirectory)
            => Sequence.Successors(startingDirectory, GetDirectoryNameOrNone)
                .FirstOrNone(DirectoryContainsFile(fileName));

        private Func<string, bool> DirectoryContainsFile(string fileName)
            => directoryPath => _fileSystem.ExistsAndIsFile(Path.Combine(directoryPath, fileName));

        private static Option<string> GetDirectoryNameOrNone(string path)
            => Option.FromNullable(Path.GetDirectoryName(path));
    }
}
