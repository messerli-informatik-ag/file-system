using System;
using System.IO;
using Funcky.Monads;

namespace Messerli.FileSystem
{
    public sealed class DirectoryLocator : IDirectoryLocator
    {
        public Option<string> FindFirstDirectoryContainingFile(string fileName, string startingDirectory)
        {
            if (fileName.Contains(Path.DirectorySeparatorChar.ToString()) || fileName.Contains(Path.AltDirectorySeparatorChar.ToString()))
            {
                throw new ArgumentException("File names should not contain directory separators", nameof(fileName));
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name should not be null", nameof(fileName));
            }

            return FindFirstDirectoryContainingFileWithCanonicalizedPath(fileName, Path.GetFullPath(startingDirectory));
        }

        private Option<string> FindFirstDirectoryContainingFileWithCanonicalizedPath(string fileName, string startingDirectory)
            => Option.Some(startingDirectory)
                .Where(path => File.Exists(Path.Combine(path, fileName)))
                .OrElse(() => FindFirstParentDirectoryContainingFile(fileName, startingDirectory));

        private Option<string> FindFirstParentDirectoryContainingFile(string fileName, string startingDirectory)
            => GetParent(startingDirectory)
                .SelectMany(parent => FindFirstDirectoryContainingFileWithCanonicalizedPath(fileName, parent));

        private static Option<string> GetParent(string path) => Option.FromNullable(Path.GetDirectoryName(path));
    }
}
