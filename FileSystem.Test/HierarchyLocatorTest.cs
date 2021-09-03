using System;
using System.IO;
using System.Linq;
using Funcky.Xunit;
using Messerli.TempDirectory;
using Xunit;

namespace Messerli.FileSystem.Test
{
    public sealed class HierarchyLocatorTest
    {
        private const string FileName = "foo.txt";

        private const string SubDirectoryName = "Directory";

        [Fact]
        public void FindFirstDirectoryContainingFile_FileInStartingDirectory_ReturnsStartingDirectory()
        {
            using var tempDirectory = new TempDirectoryBuilder().Create();
            var startingDirectory = tempDirectory.FullName;
            File.WriteAllText(Path.Combine(startingDirectory, FileName), string.Empty);

            var locator = new HierarchyLocator();
            var directory = locator.FindClosestParentDirectoryContainingFile(FileName, startingDirectory);

            FunctionalAssert.IsSome(startingDirectory, directory);
        }

        [Theory]
        [MemberData(nameof(DirectorySeparators))]
        public void FindFirstDirectoryContainingFile_FileNameContainingDirectorySeparator_ThrowsException(char separator)
        {
            using var tempDirectory = new TempDirectoryBuilder().Create();
            var fileName = $"foo{separator}bar";
            var locator = new HierarchyLocator();
            Assert.Throws<ArgumentException>(() =>
                locator.FindClosestParentDirectoryContainingFile(fileName, tempDirectory.FullName));
        }

        [Fact]
        public void FindFirstDirectoryContainingFile_EmptyFileName_ThrowsException()
        {
            using var tempDirectory = new TempDirectoryBuilder().Create();
            var locator = new HierarchyLocator();
            Assert.Throws<ArgumentException>(() =>
                locator.FindClosestParentDirectoryContainingFile(string.Empty, tempDirectory.FullName));
        }

        [Fact]
        public void FindFirstDirectoryContainingFile_NonExistentFile_ReturnsNone()
        {
            var fileName = $"non_existent_file_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.txt";
            using var tempDirectory = new TempDirectoryBuilder().Create();
            var locator = new HierarchyLocator();
            FunctionalAssert.IsNone(locator.FindClosestParentDirectoryContainingFile(fileName, tempDirectory.FullName));
        }

        public static TheoryData<char> DirectorySeparators()
            => new()
            {
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar,
            };

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(10)]
        [InlineData(50)]
        public void FindFirstDirectoryContainingFile_FileInDirectoryAboveStartingDirectory_ReturnsDirectory(int level)
        {
            using var tempDirectory = new TempDirectoryBuilder().Create();
            var startingDirectory = Path.Combine(tempDirectory.FullName, GenerateNestedPath(level));
            Directory.CreateDirectory(startingDirectory);
            File.WriteAllText(Path.Combine(tempDirectory.FullName, FileName), string.Empty);

            var locator = new HierarchyLocator();
            var directory = locator.FindClosestParentDirectoryContainingFile(FileName, startingDirectory);

            FunctionalAssert.IsSome(tempDirectory.FullName, directory);
        }

        private static string GenerateNestedPath(int level)
            => Path.Combine(Enumerable.Repeat(SubDirectoryName, level).ToArray());
    }
}
