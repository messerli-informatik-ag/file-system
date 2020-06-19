using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using Messerli.Test.Utility;
using Xunit;

namespace Messerli.FileSystem.Test
{
    public sealed class FileSystemTest
    {
        private const string NonExistentItem = "DoesNotExist";
        private const string FileName = "file.txt";
        private const string Folder = "folder";

        private static readonly string SubFolder = Path.Combine(Folder, "subfolder");
        private static readonly string FileInFolder = Path.Combine(Folder, "file.txt");
        private static readonly string FileInSubFolder = Path.Combine(SubFolder, "file.txt");

        [Fact]
        public void CheckIsDirectoryWritableOnWindows()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix ||
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                return;
            }

            var filesystem = CreateFileSystem();
            using var testEnvironment = CreateTestEnvironmentProvider();
            using var windowsIdentity = WindowsIdentity.GetCurrent();
            var denyAllRule = new FileSystemAccessRule(windowsIdentity.Name, FileSystemRights.FullControl, AccessControlType.Deny);
            var path = Path.Combine(testEnvironment.RootDirectory, Folder);
            var directoryInfo = new DirectoryInfo(path);
            Assert.True(filesystem.IsDirectoryWritable(path));

            var security = directoryInfo.GetAccessControl();
            security.AddAccessRule(denyAllRule);
            directoryInfo.SetAccessControl(security);
            var isWritable = filesystem.IsDirectoryWritable(path);
            Assert.False(isWritable);

            security.RemoveAccessRule(denyAllRule);
            directoryInfo.SetAccessControl(security);
        }

        [Theory]
        [MemberData(nameof(ChecksIfPathExistsData))]
        public void ChecksIfPathExists(string relativePath, bool expectedResult)
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var path = Path.Combine(testEnvironment.RootDirectory, relativePath);
            Assert.Equal(expectedResult, fileSystem.Exists(path));
        }

        [Fact]
        public void ChecksIfHiddenFileExists()
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var hiddenFile = Path.Combine(testEnvironment.RootDirectory, "HiddenFile.txt");
            File.Create(hiddenFile).Dispose();
            File.SetAttributes(hiddenFile, FileAttributes.Hidden);

            var fileSystem = CreateFileSystem();
            Assert.True(fileSystem.ExistsAndIsFile(hiddenFile));
        }

        [Fact]
        public void ChecksIfHiddenDirectoryExists()
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var hiddenDirectory = Path.Combine(testEnvironment.RootDirectory, "HiddenDirectory");
            var hiddenDirectoryInfo = Directory.CreateDirectory(hiddenDirectory);
            hiddenDirectoryInfo.Attributes |= FileAttributes.Hidden;

            var fileSystem = CreateFileSystem();
            Assert.True(fileSystem.ExistsAndIsDirectory(hiddenDirectory));
        }

        [Theory]
        [MemberData(nameof(ChecksIfPathExistsAndIsADirectoryData))]
        public void ChecksIfPathExistsAndIsADirectory(string relativePath, bool expectedResult)
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var path = Path.Combine(testEnvironment.RootDirectory, relativePath);
            Assert.Equal(expectedResult, fileSystem.ExistsAndIsDirectory(path));
        }

        [Theory]
        [MemberData(nameof(ChecksIfPathExistsAndIsAFileData))]
        public void ChecksIfPathExistsAndIsFile(string relativePath, bool expectedResult)
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var path = Path.Combine(testEnvironment.RootDirectory, relativePath);
            Assert.Equal(expectedResult, fileSystem.ExistsAndIsFile(path));
        }

        [Fact]
        public void DeletesFile()
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var path = Path.Combine(testEnvironment.RootDirectory, FileName);
            fileSystem.Delete(path);
            Assert.False(Directory.Exists(path));
        }

        [Fact]
        public void DeletesFolderRecursively()
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var path = Path.Combine(testEnvironment.RootDirectory, Folder);
            fileSystem.Delete(path);
            Assert.False(Directory.Exists(path));
        }

        [Fact]
        public void DeleteDoesNotThrowIfItemDoesNotExist()
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var path = Path.Combine(testEnvironment.RootDirectory, NonExistentItem);
            fileSystem.Delete(path);
        }

        [Fact]
        public void CreatesDirectory()
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var path = Path.Combine(testEnvironment.RootDirectory, "NewFolder");
            fileSystem.CreateDirectory(path);
            Assert.True(Directory.Exists(path));
        }

        [Fact]
        public void CreatesDirectoriesRecursively()
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var path = Path.Combine(testEnvironment.RootDirectory, "NewFolder", "Foo", "Bar", "Baz");
            fileSystem.CreateDirectory(path);
            Assert.True(Directory.Exists(path));
        }

        [Fact]
        public void DoesNothingIfDirectoryAlreadyExists()
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var path = Path.Combine(testEnvironment.RootDirectory, Folder);
            fileSystem.CreateDirectory(path);
            Assert.True(fileSystem.ExistsAndIsDirectory(path));
        }

        [Theory]
        [MemberData(nameof(ExistingDestinations))]
        public void MovingFileThrowsIfDestinationExists(string relativeDestination)
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var source = Path.Combine(testEnvironment.RootDirectory, FileName);
            var destination = Path.Combine(testEnvironment.RootDirectory, relativeDestination);
            Assert.Throws<IOException>(() => fileSystem.Move(source, destination));
        }

        [Theory]
        [MemberData(nameof(ExistingDestinations))]
        public void MovingDirectoryThrowsIfDestinationExists(string relativeDestination)
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var source = Path.Combine(testEnvironment.RootDirectory, Folder);
            var destination = Path.Combine(testEnvironment.RootDirectory, relativeDestination);
            Assert.Throws<IOException>(() => fileSystem.Move(source, destination));
        }

        [Fact]
        public void MoveThrowsIfDestinationIsFileAndExists()
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var source = Path.Combine(testEnvironment.RootDirectory, FileName);
            var destination = Path.Combine(testEnvironment.RootDirectory, FileInSubFolder);
            Assert.Throws<IOException>(() => fileSystem.Move(source, destination));
        }

        [Fact]
        public void MoveThrowsIfSourceDestinationAreTheSame()
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var source = Path.Combine(testEnvironment.RootDirectory, FileName);
            Assert.Throws<IOException>(() => fileSystem.Move(source, source));
        }

        [Fact]
        public void MovesFile()
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var source = Path.Combine(testEnvironment.RootDirectory, FileName);
            var destination = Path.Combine(testEnvironment.RootDirectory, "Destination.txt");
            fileSystem.Move(source, destination);
            Assert.True(File.Exists(destination));
            Assert.False(File.Exists(source));
        }

        [Fact]
        public void MovesDirectory()
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var source = Path.Combine(testEnvironment.RootDirectory, Folder);
            var destinationParent = Path.Combine(testEnvironment.RootDirectory, "Destination");
            var destination = Path.Combine(destinationParent, Folder);

            Directory.CreateDirectory(destinationParent);
            fileSystem.Move(source, destination);

            Assert.True(Directory.Exists(Path.Combine(destinationParent, Folder)));
            Assert.True(Directory.Exists(Path.Combine(destinationParent, SubFolder)));
            Assert.True(File.Exists(Path.Combine(destinationParent, FileInFolder)));
            Assert.True(File.Exists(Path.Combine(destinationParent, FileInSubFolder)));

            Assert.False(Directory.Exists(Path.Combine(testEnvironment.RootDirectory, Folder)));
            Assert.False(Directory.Exists(Path.Combine(testEnvironment.RootDirectory, SubFolder)));
            Assert.False(File.Exists(Path.Combine(testEnvironment.RootDirectory, FileInFolder)));
            Assert.False(File.Exists(Path.Combine(testEnvironment.RootDirectory, FileInSubFolder)));
        }

        [Theory]
        [MemberData(nameof(ExistingDestinations))]
        public void CopyingFileThrowsIfDestinationExists(string relativeDestination)
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var source = Path.Combine(testEnvironment.RootDirectory, FileName);
            var destination = Path.Combine(testEnvironment.RootDirectory, relativeDestination);
            Assert.Throws<IOException>(() => fileSystem.Copy(source, destination));
        }

        [Theory]
        [MemberData(nameof(ExistingDestinations))]
        public void CopyingDirectoryThrowsIfDestinationExists(string relativeDestination)
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var source = Path.Combine(testEnvironment.RootDirectory, Folder);
            var destination = Path.Combine(testEnvironment.RootDirectory, relativeDestination);
            Assert.Throws<IOException>(() => fileSystem.Copy(source, destination));
        }

        [Fact]
        public void CopyThrowsIfSourceDestinationAreTheSame()
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var source = Path.Combine(testEnvironment.RootDirectory, FileName);
            Assert.Throws<IOException>(() => fileSystem.Copy(source, source));
        }

        [Fact]
        public void CopiesFile()
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var source = Path.Combine(testEnvironment.RootDirectory, FileName);
            var destination = Path.Combine(testEnvironment.RootDirectory, "Destination.txt");
            fileSystem.Copy(source, destination);
            Assert.True(File.Exists(destination));
            Assert.True(File.Exists(source));
        }

        [Fact]
        public void CopiesDirectory()
        {
            using var testEnvironment = CreateTestEnvironmentProvider();
            var fileSystem = CreateFileSystem();
            var source = Path.Combine(testEnvironment.RootDirectory, Folder);
            var destinationParent = Path.Combine(testEnvironment.RootDirectory, "Destination");
            var destination = Path.Combine(destinationParent, Folder);

            Directory.CreateDirectory(destinationParent);
            fileSystem.Copy(source, destination);

            Assert.True(Directory.Exists(Path.Combine(destinationParent, Folder)));
            Assert.True(Directory.Exists(Path.Combine(destinationParent, SubFolder)));
            Assert.True(File.Exists(Path.Combine(destinationParent, FileInFolder)));
            Assert.True(File.Exists(Path.Combine(destinationParent, FileInSubFolder)));

            Assert.True(Directory.Exists(Path.Combine(testEnvironment.RootDirectory, Folder)));
            Assert.True(Directory.Exists(Path.Combine(testEnvironment.RootDirectory, SubFolder)));
            Assert.True(File.Exists(Path.Combine(testEnvironment.RootDirectory, FileInFolder)));
            Assert.True(File.Exists(Path.Combine(testEnvironment.RootDirectory, FileInSubFolder)));
        }

        public static TheoryData<string, bool> ChecksIfPathExistsData()
        {
            return new TheoryData<string, bool>
            {
                { FileName, true },
                { FileInFolder, true },
                { FileInSubFolder, true },
                { Folder, true },
                { SubFolder, true },
                { NonExistentItem, false },
            };
        }

        public static TheoryData<string, bool> ChecksIfPathExistsAndIsADirectoryData()
        {
            return new TheoryData<string, bool>
            {
                { Folder, true },
                { SubFolder, true },
                { FileName, false },
                { NonExistentItem, false },
            };
        }

        public static TheoryData<string, bool> ChecksIfPathExistsAndIsAFileData()
        {
            return new TheoryData<string, bool>
            {
                { FileName, true },
                { FileInFolder, true },
                { FileInSubFolder, true },
                { Folder, false },
                { NonExistentItem, false },
            };
        }

        public static TheoryData<string> ExistingDestinations()
        {
            return new TheoryData<string>
            {
                FileInFolder,
                FileInSubFolder,
                SubFolder,
            };
        }

        private static IFileSystem CreateFileSystem()
        {
            return new FileSystem();
        }

        private static TestEnvironmentProvider CreateTestEnvironmentProvider()
        {
            return new TestEnvironmentProvider(new[]
                {
                    TestFile.Create(FileName),
                    TestFile.Create(FileInFolder),
                    TestFile.Create(FileInSubFolder),
                });
        }
    }
}
