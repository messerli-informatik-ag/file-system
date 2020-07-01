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
        public IFileOpeningBuilder Create(bool create = true) => Clone(create: create);

        [Pure]
        public IFileOpeningBuilder Truncate(bool truncate = true) => Clone(truncate: truncate);

        [Pure]
        public IFileOpeningBuilder Append(bool append = true) => Clone(append: append);

        [Pure]
        public IFileOpeningBuilder Write(bool write = true) => Clone(write: write);

        [Pure]
        public IFileOpeningBuilder Read(bool read = true) => Clone(read: read);

        [Pure]
        public IFileOpeningBuilder CreateNew(bool createNew = true) => Clone(createNew: createNew);

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
            => new FileOpeningBuilder(
                create ?? _create,
                truncate ?? _truncate,
                append ?? _append,
                write ?? _write,
                read ?? _read,
                createNew ?? _createNew);

        private void HandleNotNativelySupportedConfigurations(FileInfo path)
        {
            if (_truncate && _createNew && path.Exists)
            {
                throw new IOException($"The file '{path.FullName}' already exists.");
            }

            if (_truncate && (_create || _createNew))
            {
                using (path.Create())
                {
                }
            }
        }

        private FileOpeningSettings BuildSettings()
            => new FileOpeningSettings(GetFileMode(), GetFileAccess(), FileShare.ReadWrite);

        private FileMode GetFileMode()
            => _truncate
                ? _append
                    ? throw new InvalidOperationException("Combining truncate and append makes no sense")
                    : _write
                        ? FileMode.Truncate
                        : throw new InvalidOperationException("Truncate requires write access")
                : _createNew
                    ? _write || _append
                        ? FileMode.CreateNew
                        : throw new InvalidOperationException("CreateNew requires create or append access, but only had read access")
                    : _create
                        ? FileMode.OpenOrCreate
                        : _append
                            ? FileMode.Append
                            : FileMode.Open;

        private FileAccess GetFileAccess()
            => _read
                ? _write
                    ? FileAccess.ReadWrite
                    : FileAccess.Read
                : _write || _append
                    ? FileAccess.Write
                    : throw new InvalidOperationException($"No file access has been specified. {Environment.NewLine}" +
                                                          "Specify at least one of the following accesses: read, write, append");

        [CustomEqualsInternal]
        [SuppressMessage("Code Quality", "IDE0051")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private bool CustomEquals(FileOpeningBuilder other)
            => _create == other._create &&
               _truncate == other._truncate &&
               _append == other._append &&
               _write == other._write &&
               _read == other._read &&
               _createNew == other._createNew;

        private readonly struct FileOpeningSettings
        {
            public readonly FileMode FileMode;

            public readonly FileAccess FileAccess;

            public readonly FileShare FileShare;

            public FileOpeningSettings(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
            {
                FileMode = fileMode;
                FileAccess = fileAccess;
                FileShare = fileShare;
            }
        }
    }
}
