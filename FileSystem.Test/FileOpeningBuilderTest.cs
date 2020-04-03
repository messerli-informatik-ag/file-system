using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Messerli.Test.Utility;
using Xunit;

namespace Messerli.FileSystem.Test
{
    public class FileOpeningBuilderTest
    {
        private const string DirectoryName = "FileOpeningBuilderTest";

        private const string SubDirectoryName = "directory";

        private const string ContentToWrite = "wobble\n";

        private static readonly TestFileData RegularFile =
            new TestFileData("regular_file.txt", "foo\n");

        private static readonly TestFileData ReadOnlyFile =
            new TestFileData("readonly_file.txt", "bar\n");

        private static readonly TestFileData HiddenFile =
            new TestFileData("hidden_file.txt", "baz\n");

        private static readonly TestFileData NonExistentFile =
            new TestFileData("nonexistent_file.txt", ContentToWrite);

        private static readonly TestFileData NestedFile =
            new TestFileData(Path.Combine(SubDirectoryName, "nested_file.txt"), string.Empty);

        private delegate string GetTestFile(string fileName);

        [Fact]
        public void ThrowsWhenNoOptionsHaveBeenSpecified()
        {
            using (var testEnvironment = new TestEnvironmentProvider())
            {
                var file = Path.Combine(testEnvironment.RootDirectory, NonExistentFile.Name);
                Assert.Throws<InvalidOperationException>(() =>
                {
                    using (new FileOpeningBuilder().Open(file))
                    {
                    }
                });
            }
        }

        [Fact]
        public void ThrowsWhenReadingNonExistentFile()
        {
            using (var testEnvironment = new TestEnvironmentProvider())
            {
                var file = Path.Combine(testEnvironment.RootDirectory, NonExistentFile.Name);
                Assert.Throws<FileNotFoundException>(() =>
                {
                    using (new FileOpeningBuilder()
                        .Read(true)
                        .Open(file))
                    {
                    }
                });
            }
        }

        [Theory]
        [MemberData(nameof(GetReadableFiles))]
        public void ReadsFiles(TestFileData testFileData)
        {
            using (var testEnvironmentProvider = CreateTestEnvironmentProvider())
            {
                var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
                AssertThatFileContains(getTestFilePath(testFileData.Name), testFileData.Content);
            }
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void ReadsWritableFiles(TestFileData testFileData)
        {
            using (var testEnvironmentProvider = CreateTestEnvironmentProvider())
            {
                var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
                var builder = new FileOpeningBuilder()
                    .Read(true)
                    .Write(true);
                AssertThatFileContains(builder, getTestFilePath(testFileData.Name), testFileData.Content);
            }
        }

        [Fact]
        public void ThrowsWhenWritingReadonlyFileFile()
        {
            using (var testEnvironmentProvider = CreateTestEnvironmentProvider())
            {
                var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
                Assert.Throws<UnauthorizedAccessException>(() =>
                {
                    using (new FileOpeningBuilder()
                        .Write(true)
                        .Open(getTestFilePath(ReadOnlyFile.Name)))
                    {
                    }
                });
            }
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void WritingOnExistingFileOverwritesContents(TestFileData testFileData)
        {
            var builder = new FileOpeningBuilder()
                .Write(true);
            AssertThatFileContainsWrittenContent(builder, testFileData, ContentToWrite);
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void ReadingOverwrittenFileReturnsEmptyContent(TestFileData testFileData)
        {
            using (var testEnvironmentProvider = CreateTestEnvironmentProvider())
            {
                var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
                using (var stream = new FileOpeningBuilder()
                    .Read(true)
                    .Write(true)
                    .Open(getTestFilePath(testFileData.Name)))
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(ContentToWrite);
                    stream.Write(bytes);

                    var content = ReadStream(stream);
                    Assert.Empty(content);
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void OpeningFileWithWriteAccessWithoutWritingDoesNotOverwriteContents(TestFileData testFileData)
        {
            using (var testEnvironmentProvider = CreateTestEnvironmentProvider())
            {
                var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
                using (new FileOpeningBuilder()
                    .Write(true)
                    .Open(getTestFilePath(testFileData.Name)))
                {
                }

                AssertThatFileContains(getTestFilePath(testFileData.Name), testFileData.Content);
            }
        }

        [Fact]
        public void ThrowsWhenUsingCreateWithoutReadOrWrite()
        {
            using (var testEnvironmentProvider = CreateTestEnvironmentProvider())
            {
                var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
                Assert.Throws<InvalidOperationException>(() =>
                {
                    using (new FileOpeningBuilder()
                        .Create(true)
                        .Open(getTestFilePath(NonExistentFile.Name)))
                    {
                    }
                });
            }
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void WritingToFileWithCreateAndWriteOverwritesFile(TestFileData testFileData)
        {
            var builder = new FileOpeningBuilder()
                .Write(true)
                .Create(true);
            AssertThatFileContainsWrittenContent(builder, testFileData, ContentToWrite);
        }

        [Fact]
        public void CreatesAndWritesNewFile()
        {
            var builder = new FileOpeningBuilder()
                .Write(true)
                .Create(true);
            AssertThatFileContainsWrittenContent(builder, NonExistentFile, ContentToWrite);
        }

        [Fact]
        public void CreatesAndReadsNewFile()
        {
            using (var testEnvironmentProvider = CreateTestEnvironmentProvider())
            {
                var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
                var builder = new FileOpeningBuilder()
                    .Read(true)
                    .Create(true);
                AssertThatFileContains(builder, getTestFilePath(NonExistentFile.Name), string.Empty);
            }
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void AppendsToExistingFile(TestFileData testFileData)
        {
            using (var testEnvironmentProvider = CreateTestEnvironmentProvider())
            {
                var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
                using (var stream = new FileOpeningBuilder()
                        .Append(true)
                        .Open(getTestFilePath(testFileData.Name)))
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(ContentToWrite);
                    stream.Write(bytes);
                }

                var expectedContent = testFileData.Content + ContentToWrite;
                AssertThatFileContains(getTestFilePath(testFileData.Name), expectedContent);
            }
        }

        [Fact]
        public void ThrowsWhenAppendingToReadonlyFile()
        {
            using (var testEnvironmentProvider = CreateTestEnvironmentProvider())
            {
                var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
                Assert.Throws<UnauthorizedAccessException>(() =>
                {
                    using (new FileOpeningBuilder()
                        .Append(true)
                        .Open(getTestFilePath(ReadOnlyFile.Name)))
                    {
                    }
                });
            }
        }

        [Theory]
        [MemberData(nameof(GetReadableFiles))]
        public void ThrowsWhenForceCreatingExistingFile(TestFileData testFileData)
        {
            using (var testEnvironmentProvider = CreateTestEnvironmentProvider())
            {
                var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
                Assert.Throws<IOException>(() =>
                {
                    using (new FileOpeningBuilder()
                        .CreateNew(true)
                        .Write(true)
                        .Open(getTestFilePath(testFileData.Name)))
                    {
                    }
                });
            }
        }

        [Fact]
        public void ThrowsWhenForceCreatingWithOnlyReadAccess()
        {
            using (var testEnvironmentProvider = CreateTestEnvironmentProvider())
            {
                var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
                Assert.Throws<InvalidOperationException>(() =>
                {
                    using (new FileOpeningBuilder()
                        .CreateNew(true)
                        .Read(true)
                        .Open(getTestFilePath(NonExistentFile.Name)))
                    {
                    }
                });
            }
        }

        [Fact]
        public void ForceCreatesAndWritesFile()
        {
            var builder = new FileOpeningBuilder()
                .Write(true)
                .CreateNew(true);
            AssertThatFileContainsWrittenContent(builder, NonExistentFile, ContentToWrite);
        }

        [Fact]
        public void ForceCreatesAndWritesFileWhenUsingRedundantConfigurations()
        {
            var builder = new FileOpeningBuilder()
                .Read(true)
                .Write(true)
                .Create(true)
                .Truncate(true)
                .CreateNew(true);
            AssertThatFileContainsWrittenContent(builder, NonExistentFile, ContentToWrite);
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void TruncatingFileWithoutWritingMakesItEmpty(TestFileData testFileData)
        {
            using (var testEnvironmentProvider = CreateTestEnvironmentProvider())
            {
                var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
                using (new FileOpeningBuilder()
                    .Write(true)
                    .Truncate(true)
                    .Open(getTestFilePath(testFileData.Name)))
                {
                }

                AssertThatFileContains(getTestFilePath(testFileData.Name), string.Empty);
            }
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void ReadsTruncatedFileDirectly(TestFileData testFileData)
        {
            using (var testEnvironmentProvider = CreateTestEnvironmentProvider())
            {
                var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
                var builder = new FileOpeningBuilder()
                    .Write(true)
                    .Read(true)
                    .Truncate(true);
                AssertThatFileContains(builder, getTestFilePath(testFileData.Name), string.Empty);
            }
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void ThrowsWhenTruncatingWithOnlyReadAccess(TestFileData testFileData)
        {
            using (var testEnvironmentProvider = CreateTestEnvironmentProvider())
            {
                var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
                Assert.Throws<InvalidOperationException>(() =>
                {
                    using (new FileOpeningBuilder()
                        .Truncate(true)
                        .Read(true)
                        .Open(getTestFilePath(testFileData.Name)))
                    {
                    }
                });
            }
        }

        [Fact]
        public void ThrowsWhenTruncatingReadonlyFile()
        {
            using (var testEnvironmentProvider = CreateTestEnvironmentProvider())
            {
                var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
                Assert.Throws<UnauthorizedAccessException>(() =>
                {
                    using (new FileOpeningBuilder()
                        .Truncate(true)
                        .Write(true)
                        .Open(getTestFilePath(ReadOnlyFile.Name)))
                    {
                    }
                });
            }
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void ThrowsWhenCombiningTruncateAndAppend(TestFileData testFileData)
        {
            using (var testEnvironmentProvider = CreateTestEnvironmentProvider())
            {
                var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
                Assert.Throws<InvalidOperationException>(() =>
                {
                    using (
                        new FileOpeningBuilder()
                            .Truncate(true)
                            .Append(true)
                            .Open(getTestFilePath(testFileData.Name)))
                    {
                    }
                });
            }
        }

        [Fact]
        public void TruncatedNewFileCanBeWrittenTo()
        {
            using (var testEnvironementProvider = CreateTestEnvironmentProvider())
            {
                SetupTestEnvironment(testEnvironementProvider.RootDirectory);
                var builder = new FileOpeningBuilder()
                    .Write(true)
                    .Create(true)
                    .Truncate(true);
                AssertThatFileContainsWrittenContent(builder, NonExistentFile, ContentToWrite);
            }
        }

        [Theory]
        [MemberData(nameof(Modifiers))]
        public void CallingModifiersCreatesACopy(Func<IFileOpeningBuilder, IFileOpeningBuilder> applyModifier)
        {
            var builderOne = new Messerli.FileSystem.FileOpeningBuilder();
            var builderTwo = applyModifier(builderOne);
            Assert.NotEqual(builderOne, builderTwo);
        }

        [Theory]
        [MemberData(nameof(Modifiers))]
        public void BuilderIsEqualWhenTheSameFlagsAreSet(Func<IFileOpeningBuilder, IFileOpeningBuilder> applyModifier)
        {
            var builderOne = applyModifier(new FileOpeningBuilder());
            var builderTwo = applyModifier(new FileOpeningBuilder());
            Assert.Equal(builderOne, builderTwo);
        }

        public static TheoryData<Func<IFileOpeningBuilder, IFileOpeningBuilder>> Modifiers()
        {
            return new TheoryData<Func<IFileOpeningBuilder, IFileOpeningBuilder>>
            {
                b => b.Create(true),
                b => b.Truncate(true),
                b => b.Append(true),
                b => b.Write(true),
                b => b.Read(true),
                b => b.CreateNew(true),
            };
        }

        public static IEnumerable<object[]> GetReadableFiles()
        {
            yield return new object[] { RegularFile };
            yield return new object[] { ReadOnlyFile };
            yield return new object[] { HiddenFile };
            yield return new object[] { NestedFile };
        }

        public static IEnumerable<object[]> GetWritableFiles()
        {
            yield return new object[] { RegularFile };
            yield return new object[] { HiddenFile };
            yield return new object[] { NestedFile };
        }

        private static void AssertThatFileContainsWrittenContent(
            IFileOpeningBuilder fileOpeningBuilder,
            TestFileData testFileData,
            string contentToWrite)
        {
            using (var testEnvironementProvider = CreateTestEnvironmentProvider())
            {
                var getTestFilePath = SetupTestEnvironment(testEnvironementProvider.RootDirectory);
                using (var stream = fileOpeningBuilder
                    .Open(getTestFilePath(testFileData.Name)))
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(contentToWrite);
                    stream.Write(bytes);
                }

                AssertThatFileContains(getTestFilePath(testFileData.Name), contentToWrite);
            }
        }

        private static void AssertThatFileContains(string path, string expectedContent)
        {
            var builder = new FileOpeningBuilder().Read(true);
            AssertThatFileContains(builder, path, expectedContent);
        }

        private static void AssertThatFileContains(
            IFileOpeningBuilder builder,
            string path,
            string expectedContent)
        {
            using (var stream = builder.Open(path))
            {
                var content = ReadStream(stream);
                Assert.Equal(expectedContent, content);
            }
        }

        private static GetTestFile SetupTestEnvironment(string rootDirectory)
        {
            var getTestFile = CreateGetTestFilePath(rootDirectory);

            var fileToAttributes = new Dictionary<string, FileAttributes>
            {
                { RegularFile.Name, FileAttributes.Normal },
                { ReadOnlyFile.Name, FileAttributes.ReadOnly },
                { HiddenFile.Name, FileAttributes.Hidden },
                { SubDirectoryName, FileAttributes.Directory },
            };

            foreach (var (file, attributes) in fileToAttributes)
            {
                var fileInfo = new FileInfo(getTestFile(file))
                {
                    Attributes = attributes,
                };
            }

            return getTestFile;
        }

        private static TestEnvironmentProvider CreateTestEnvironmentProvider()
        {
            var testFiles = CreateTestFiles().ToArray();
            return new TestEnvironmentProvider(testFiles);
        }

        private static GetTestFile CreateGetTestFilePath(string root)
        {
            return fileName => Path.Combine(root, DirectoryName, fileName);
        }

        private static IEnumerable<TestFile> CreateTestFiles()
        {
            return new[]
                {
                    RegularFile.Name,
                    ReadOnlyFile.Name,
                    HiddenFile.Name,
                    NestedFile.Name,
                }
                .Select(fileName => Path.Combine(DirectoryName, fileName))
                .Select(path => new TestFile(path, path));
        }

        private static string ReadStream(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader
                    .ReadToEnd()

                    // For some reason, the stream reader sometimes
                    // lies about the newlines it encountered
                    .Replace(Environment.NewLine, "\n");
            }
        }

        public readonly struct TestFileData
        {
            public readonly string Name;

            public readonly string Content;

            public TestFileData(string name, string content)
            {
                Name = name;
                Content = content;
            }
        }
    }
}
