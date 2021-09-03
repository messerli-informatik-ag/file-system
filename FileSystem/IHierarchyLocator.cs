using Funcky.Monads;

namespace Messerli.FileSystem
{
    public interface IHierarchyLocator
    {
        Option<string> FindFirstDirectoryContainingFile(string fileName, string startingDirectory);
    }
}
