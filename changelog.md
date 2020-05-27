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
