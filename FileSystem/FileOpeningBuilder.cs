using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace Messerli.FileSystem
{
    public record FileOpeningBuilder : IFileOpeningBuilder
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

        [Pure]
        public IFileOpeningBuilder Create(bool create = true) => ShallowClone(create: create);

        [Pure]
        public IFileOpeningBuilder Truncate(bool truncate = true) => ShallowClone(truncate: truncate);

        [Pure]
        public IFileOpeningBuilder Append(bool append = true) => ShallowClone(append: append);

        [Pure]
        public IFileOpeningBuilder Write(bool write = true) => ShallowClone(write: write);

        [Pure]
        public IFileOpeningBuilder Read(bool read = true) => ShallowClone(read: read);

        [Pure]
        public IFileOpeningBuilder CreateNew(bool createNew = true) => ShallowClone(createNew: createNew);

        public Stream Open(string path)
        {
            var settings = BuildSettings();
            var fileInfo = new FileInfo(path);
            HandleNotNativelySupportedConfigurations(fileInfo);
            return fileInfo.Open(settings.FileMode, settings.FileAccess, settings.FileShare);
        }

        [Pure]
        private IFileOpeningBuilder ShallowClone(
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
