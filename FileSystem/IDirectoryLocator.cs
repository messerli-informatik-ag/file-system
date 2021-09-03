using Funcky.Monads;

namespace Messerli.FileSystem
{
    public interface IDirectoryLocator
    {
        Option<string> FindFirstDirectoryContainingFile(string fileName, string startingDirectory);
    }
}
