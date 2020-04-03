using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Messerli.FileSystem
{
    public sealed class FileSystem : IFileSystem
    {
        public bool Exists(string path)
            => ExistsAndIsDirectory(path) || ExistsAndIsFile(path);

        public bool ExistsAndIsDirectory(string path)
            => Directory.Exists(path);

        public bool ExistsAndIsFile(string path)
            => File.Exists(path);

        [SuppressMessage("ReSharper", "ArgumentsStyleLiteral")]
        public void Delete(string path)
        {
            if (ExistsAndIsDirectory(path))
            {
                Directory.Delete(path, recursive: true);
            }
            else
            {
                File.Delete(path);
            }
        }

        public void Move(string source, string destination)
        {
            AssertThatSourceAndDestinationAreNotTheSame(source, destination);

            if (ExistsAndIsDirectory(source))
            {
                Directory.Move(source, destination);
            }
            else
            {
                File.Move(source, destination);
            }
        }

        public void Copy(string source, string destination)
        {
            AssertThatSourceAndDestinationAreNotTheSame(source, destination);

            if (ExistsAndIsDirectory(source))
            {
                CopyDirectory(source, destination);
            }
            else
            {
                File.Copy(source, destination);
            }
        }

        public void CreateDirectory(string path)
            => Directory.CreateDirectory(path);

        public IEnumerable<string> GetFiles(string path)
            => Directory.GetFiles(path);

        public IEnumerable<string> GetFiles(string path, string searchPattern)
            => Directory.GetFiles(path, searchPattern);

        private static void AssertThatSourceAndDestinationAreNotTheSame(string source, string destination)
        {
            var fullSourcePath = Path.GetFullPath(source);
            var fullDestinationPath = Path.GetFullPath(destination);

            if (fullSourcePath == fullDestinationPath)
            {
                throw new IOException($"{nameof(source)} and {nameof(destination)} must not be the same path");
            }
        }

        private static void CopyDirectory(string source, string destination)
        {
            var directoryInfo = new DirectoryInfo(source);
            Directory.CreateDirectory(destination);
            CopyFilesInDirectory(directoryInfo, destination);
            CopySubdirectories(directoryInfo, destination);
        }

        private static void CopyFilesInDirectory(DirectoryInfo source, string destination)
        {
            foreach (var file in source.GetFiles())
            {
                var destinationFile = Path.Combine(destination, file.Name);
                file.CopyTo(destinationFile);
            }
        }

        private static void CopySubdirectories(DirectoryInfo source, string destination)
        {
            foreach (var subdirectory in source.GetDirectories())
            {
                var destinationSubdirectory = Path.Combine(destination, subdirectory.Name);
                CopyDirectory(subdirectory.FullName, destinationSubdirectory);
            }
        }
    }
}
