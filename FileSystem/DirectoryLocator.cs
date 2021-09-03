using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Funcky;
using Funcky.Extensions;
using Funcky.Monads;

namespace Messerli.FileSystem
{
    public sealed class DirectoryLocator : IDirectoryLocator
    {
        public Option<string> FindFirstDirectoryContainingFile(string fileName, string startingDirectory)
        {
            if (Path.GetInvalidFileNameChars().Any(fileName.Contains))
            {
                throw new ArgumentException("File name contains invalid characters", nameof(fileName));
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name should not be empty", nameof(fileName));
            }

            return FindFirstDirectoryContainingFileWithCanonicalizedPath(fileName, Path.GetFullPath(startingDirectory));
        }

        private static Option<string> FindFirstDirectoryContainingFileWithCanonicalizedPath(string fileName, string startingDirectory)
            => Sequence.Return(startingDirectory)
                .Concat(ParentDirectories(startingDirectory))
                .FirstOrNone(DirectoryContainsFile(fileName));

        private static Func<string, bool> DirectoryContainsFile(string fileName)
            => directoryPath => File.Exists(Path.Combine(directoryPath, fileName));

        private static IEnumerable<string> ParentDirectories(string path)
            => Sequence.Generate(path, p => Option.FromNullable(Path.GetDirectoryName(p)));
    }
}
