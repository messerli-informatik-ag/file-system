# Changelog

## 0.1.0
Initial release

## 0.1.1
- Added `FileOpeningBuilder`

## 0.1.2
- Add support for .NET Standard 2.0.

## 0.1.3
- `FileOpeningBuilder` now always throws when trying to open an existing file with `CreateNew` enabled.
  The exception was previously not thrown when `Truncate` was enabled.
- Boolean parameters in `IFileOpeningBuilder` are now optional and default to `true`.

## 0.1.4
- Make boolean parameters in `FileOpeningBuilder` optional too.

## 0.1.5
- Now you can use `FileSystem` to test whether a directory is writeable or not (`DirectoryIsWritable`). A dummy file is created for this purpose in the given path.

## 0.1.6
- Add documentation for `IFileOpeningBuilder`.
- Add stub for `IFileOpeningBuilder`: `FileOpeningBuilderStub`.

## 0.1.7
- `FileOpeningBuilderStub` now optionally accepts a custom factory for creating a stream:
   ```csharp
   using var memoryStream = new MemoryStream();
   var fileOpeningBuilder = new FileOpeningBuilderStub(memoryStream.Borrow);
   ```

## 0.1.8
* Add a new helper function `IHierarchyLocator.FindClosestParentDirectoryContainingFile`
  that finds the closest directory that contains a file with the given name.
