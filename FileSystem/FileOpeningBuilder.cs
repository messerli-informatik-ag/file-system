#pragma warning disable 660,661

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;

namespace Messerli.FileSystem
{
    [Equals]
    public class FileOpeningBuilder : IFileOpeningBuilder
    {
        private readonly bool _create;
        private readonly bool _truncate;
        private readonly bool _append;
        private readonly bool _write;
        private readonly bool _read;
        private readonly bool _createNew;

        public FileOpeningBuilder()
        {
        }

        private FileOpeningBuilder(bool create, bool truncate, bool append, bool write, bool read, bool createNew)
        {
            _create = create;
            _truncate = truncate;
            _append = append;
            _write = write;
            _read = read;
            _createNew = createNew;
        }

        public static bool operator ==(FileOpeningBuilder left, FileOpeningBuilder right) => Operator.Weave(left, right);

        public static bool operator !=(FileOpeningBuilder left, FileOpeningBuilder right) => Operator.Weave(left, right);

        [Pure]
        public IFileOpeningBuilder Create(bool create) => Clone(create: create);

        [Pure]
        public IFileOpeningBuilder Truncate(bool truncate) => Clone(truncate: truncate);

        [Pure]
        public IFileOpeningBuilder Append(bool append) => Clone(append: append);

        [Pure]
        public IFileOpeningBuilder Write(bool write) => Clone(write: write);

        [Pure]
        public IFileOpeningBuilder Read(bool read) => Clone(read: read);

        [Pure]
        public IFileOpeningBuilder CreateNew(bool createNew) => Clone(createNew: createNew);

        public Stream Open(string path)
        {
            var settings = BuildSettings();
            var fileInfo = new FileInfo(path);
            HandleNotNativelySupportedConfigurations(fileInfo);
            return fileInfo.Open(settings.FileMode, settings.FileAccess, settings.FileShare);
        }

        [Pure]
        private IFileOpeningBuilder Clone(
            bool? create = null,
            bool? truncate = null,
            bool? append = null,
            bool? write = null,
            bool? read = null,
            bool? createNew = null)
        {
            return new FileOpeningBuilder(
                create ?? _create,
                truncate ?? _truncate,
                append ?? _append,
                write ?? _write,
                read ?? _read,
                createNew ?? _createNew);
        }

        private void HandleNotNativelySupportedConfigurations(FileInfo path)
        {
            if (!_truncate || (!_create && !_createNew))
            {
                return;
            }

            using (path.Create())
            {
            }
        }

        private FileOpeningSettings BuildSettings()
        {
            var fileAccess =
                _read
                    ? _write
                        ? FileAccess.ReadWrite
                        : FileAccess.Read
                    : _write || _append
                        ? FileAccess.Write
                        : throw new InvalidOperationException(
                            "No file access has been specified." + Environment.NewLine +
                            "Specify at least one of the following accesses: read, write, append");

            var fileMode =
                _truncate
                    ? _append
                        ? throw new InvalidOperationException("Combining truncate and append makes no sense")
                        : _write
                            ? FileMode.Truncate
                            : throw new InvalidOperationException("Truncate requires write access")
                    : _createNew
                        ? _write || _append
                            ? FileMode.CreateNew
                            : throw new InvalidOperationException(
                                "CreateNew requires create or append access, but only had read access")
                        : _create
                            ? FileMode.OpenOrCreate
                            : _append
                                ? FileMode.Append
                                : FileMode.Open;

            const FileShare fileShare = FileShare.ReadWrite;

            return new FileOpeningSettings(fileMode, fileAccess, fileShare);
        }

        [CustomEqualsInternal]
        [SuppressMessage("Code Quality", "IDE0051")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private bool CustomEquals(FileOpeningBuilder other)
        {
            return _create == other._create &&
                   _truncate == other._truncate &&
                   _append == other._append &&
                   _write == other._write &&
                   _read == other._read &&
                   _createNew == other._createNew;
        }

        private struct FileOpeningSettings
        {
            public FileOpeningSettings(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
            {
                FileMode = fileMode;
                FileAccess = fileAccess;
                FileShare = fileShare;
            }

            public FileMode FileMode { get; }

            public FileAccess FileAccess { get; }

            public FileShare FileShare { get; }
        }
    }
}
