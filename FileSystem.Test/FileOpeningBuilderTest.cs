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
            using var testEnvironment = new TestEnvironmentProvider();
            var file = Path.Combine(testEnvironment.RootDirectory, NonExistentFile.Name);
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (new FileOpeningBuilder().Open(file))
                {
                }
            });
        }

        [Fact]
        public void ThrowsWhenReadingNonExistentFile()
        {
            using var testEnvironment = new TestEnvironmentProvider();
            var file = Path.Combine(testEnvironment.RootDirectory, NonExistentFile.Name);
            Assert.Throws<FileNotFoundException>(() =>
            {
                using (new FileOpeningBuilder()
                    .Read()
                    .Open(file))
                {
                }
            });
        }

        [Theory]
        [MemberData(nameof(GetReadableFiles))]
        public void ReadsFiles(TestFileData testFileData)
        {
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            AssertThatFileContains(getTestFilePath(testFileData.Name), testFileData.Content);
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void ReadsWritableFiles(TestFileData testFileData)
        {
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            var builder = new FileOpeningBuilder()
                .Read()
                .Write();
            AssertThatFileContains(builder, getTestFilePath(testFileData.Name), testFileData.Content);
        }

        [Fact]
        public void ThrowsWhenWritingReadonlyFileFile()
        {
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            Assert.Throws<UnauthorizedAccessException>(() =>
            {
                using (new FileOpeningBuilder()
                    .Write()
                    .Open(getTestFilePath(ReadOnlyFile.Name)))
                {
                }
            });
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void WritingOnExistingFileOverwritesContents(TestFileData testFileData)
        {
            var builder = new FileOpeningBuilder()
                .Write();
            AssertThatFileContainsWrittenContent(builder, testFileData, ContentToWrite);
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void ReadingOverwrittenFileReturnsEmptyContent(TestFileData testFileData)
        {
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            using var stream = new FileOpeningBuilder()
                .Read()
                .Write()
                .Open(getTestFilePath(testFileData.Name));
            var bytes = System.Text.Encoding.UTF8.GetBytes(ContentToWrite);
            stream.Write(bytes);

            var content = ReadStream(stream);
            Assert.Empty(content);
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void OpeningFileWithWriteAccessWithoutWritingDoesNotOverwriteContents(TestFileData testFileData)
        {
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            using (new FileOpeningBuilder()
                .Write()
                .Open(getTestFilePath(testFileData.Name)))
            {
            }

            AssertThatFileContains(getTestFilePath(testFileData.Name), testFileData.Content);
        }

        [Fact]
        public void ThrowsWhenUsingCreateWithoutReadOrWrite()
        {
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (new FileOpeningBuilder()
                    .Create()
                    .Open(getTestFilePath(NonExistentFile.Name)))
                {
                }
            });
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void WritingToFileWithCreateAndWriteOverwritesFile(TestFileData testFileData)
        {
            var builder = new FileOpeningBuilder()
                .Write()
                .Create();
            AssertThatFileContainsWrittenContent(builder, testFileData, ContentToWrite);
        }

        [Fact]
        public void CreatesAndWritesNewFile()
        {
            var builder = new FileOpeningBuilder()
                .Write()
                .Create();
            AssertThatFileContainsWrittenContent(builder, NonExistentFile, ContentToWrite);
        }

        [Fact]
        public void CreatesAndReadsNewFile()
        {
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            var builder = new FileOpeningBuilder()
                .Read()
                .Create();
            AssertThatFileContains(builder, getTestFilePath(NonExistentFile.Name), string.Empty);
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void AppendsToExistingFile(TestFileData testFileData)
        {
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            using (var stream = new FileOpeningBuilder()
                .Append()
                .Open(getTestFilePath(testFileData.Name)))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(ContentToWrite);
                stream.Write(bytes);
            }

            var expectedContent = testFileData.Content + ContentToWrite;
            AssertThatFileContains(getTestFilePath(testFileData.Name), expectedContent);
        }

        [Fact]
        public void ThrowsWhenAppendingToReadonlyFile()
        {
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            Assert.Throws<UnauthorizedAccessException>(() =>
            {
                using (new FileOpeningBuilder()
                    .Append()
                    .Open(getTestFilePath(ReadOnlyFile.Name)))
                {
                }
            });
        }

        [Theory]
        [MemberData(nameof(GetReadableFiles))]
        public void ThrowsWhenForceCreatingExistingFile(TestFileData testFileData)
        {
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            Assert.Throws<IOException>(() =>
            {
                using (new FileOpeningBuilder()
                    .CreateNew()
                    .Write()
                    .Open(getTestFilePath(testFileData.Name)))
                {
                }
            });
        }

        [Fact]
        public void ThrowsWhenForceCreatingWithOnlyReadAccess()
        {
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (new FileOpeningBuilder()
                    .CreateNew()
                    .Read()
                    .Open(getTestFilePath(NonExistentFile.Name)))
                {
                }
            });
        }

        [Fact]
        public void ForceCreatesAndWritesFile()
        {
            var builder = new FileOpeningBuilder()
                .Write()
                .CreateNew();
            AssertThatFileContainsWrittenContent(builder, NonExistentFile, ContentToWrite);
        }

        [Fact]
        public void ForceCreatesAndWritesFileWhenUsingRedundantConfigurations()
        {
            var builder = new FileOpeningBuilder()
                .Read()
                .Write()
                .Create()
                .Truncate()
                .CreateNew();
            AssertThatFileContainsWrittenContent(builder, NonExistentFile, ContentToWrite);
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void TruncatingFileWithoutWritingMakesItEmpty(TestFileData testFileData)
        {
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            using (new FileOpeningBuilder()
                .Write()
                .Truncate()
                .Open(getTestFilePath(testFileData.Name)))
            {
            }

            AssertThatFileContains(getTestFilePath(testFileData.Name), string.Empty);
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void ReadsTruncatedFileDirectly(TestFileData testFileData)
        {
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            var builder = new FileOpeningBuilder()
                .Write()
                .Read()
                .Truncate();
            AssertThatFileContains(builder, getTestFilePath(testFileData.Name), string.Empty);
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void ThrowsWhenTruncatingWithOnlyReadAccess(TestFileData testFileData)
        {
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (new FileOpeningBuilder()
                    .Truncate()
                    .Read()
                    .Open(getTestFilePath(testFileData.Name)))
                {
                }
            });
        }

        [Fact]
        public void ThrowsWhenTruncatingReadonlyFile()
        {
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            Assert.Throws<UnauthorizedAccessException>(() =>
            {
                using (new FileOpeningBuilder()
                    .Truncate()
                    .Write()
                    .Open(getTestFilePath(ReadOnlyFile.Name)))
                {
                }
            });
        }

        [Theory]
        [MemberData(nameof(GetWritableFiles))]
        public void ThrowsWhenCombiningTruncateAndAppend(TestFileData testFileData)
        {
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (
                    new FileOpeningBuilder()
                        .Truncate()
                        .Append()
                        .Open(getTestFilePath(testFileData.Name)))
                {
                }
            });
        }

        [Fact]
        public void TruncatedNewFileCanBeWrittenTo()
        {
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            var builder = new FileOpeningBuilder()
                .Write()
                .Create()
                .Truncate();
            AssertThatFileContainsWrittenContent(builder, NonExistentFile, ContentToWrite);
        }

        [Theory]
        [MemberData(nameof(Modifiers))]
        public void CallingModifiersCreatesACopy(Func<IFileOpeningBuilder, IFileOpeningBuilder> applyModifier)
        {
            var builderOne = new FileOpeningBuilder();
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

        [Fact]
        public void CreateNewThrowsWhenFileAlreadyExistsEvenIfTruncateIsSet()
        {
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            var builder = new FileOpeningBuilder()
                .Write()
                .CreateNew()
                .Truncate();
            Assert.Throws<IOException>(() =>
            {
                using var stream = builder.Open(getTestFilePath(RegularFile.Name));
            });
        }

        public static TheoryData<Func<IFileOpeningBuilder, IFileOpeningBuilder>> Modifiers()
            => new TheoryData<Func<IFileOpeningBuilder, IFileOpeningBuilder>>
            {
                b => b.Create(),
                b => b.Truncate(),
                b => b.Append(),
                b => b.Write(),
                b => b.Read(),
                b => b.CreateNew(),
            };

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
            using var testEnvironmentProvider = CreateTestEnvironmentProvider();
            var getTestFilePath = SetupTestEnvironment(testEnvironmentProvider.RootDirectory);
            using (var stream = fileOpeningBuilder
                .Open(getTestFilePath(testFileData.Name)))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(contentToWrite);
                stream.Write(bytes);
            }

            AssertThatFileContains(getTestFilePath(testFileData.Name), contentToWrite);
        }

        private static void AssertThatFileContains(string path, string expectedContent)
        {
            var builder = new FileOpeningBuilder().Read();
            AssertThatFileContains(builder, path, expectedContent);
        }

        private static void AssertThatFileContains(
            IFileOpeningBuilder builder,
            string path,
            string expectedContent)
        {
            using var stream = builder.Open(path);
            var content = ReadStream(stream);
            Assert.Equal(expectedContent, content);
        }

        private static GetTestFile SetupTestEnvironment(string rootDirectory)
        {
            var getTestFile = CreateGetTestFilePath(rootDirectory);

            var fileToAttributes = new Dictionary<string, FileAttributes>
            {
                [RegularFile.Name] = FileAttributes.Normal,
                [ReadOnlyFile.Name] = FileAttributes.ReadOnly,
                [HiddenFile.Name] = FileAttributes.Hidden,
                [SubDirectoryName] = FileAttributes.Directory,
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
            => fileName => Path.Combine(root, DirectoryName, fileName);

        private static IEnumerable<TestFile> CreateTestFiles()
            => new[]
            {
                RegularFile.Name,
                ReadOnlyFile.Name,
                HiddenFile.Name,
                NestedFile.Name,
            }
            .Select(fileName => Path.Combine(DirectoryName, fileName))
            .Select(path => new TestFile(path, path));

        private static string ReadStream(Stream stream)
        {
            using var streamReader = new StreamReader(stream);
            return streamReader
                .ReadToEnd()

                // For some reason, the stream reader sometimes
                // lies about the newlines it encountered
                .Replace(Environment.NewLine, "\n");
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
