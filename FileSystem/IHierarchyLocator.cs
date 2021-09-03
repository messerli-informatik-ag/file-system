using System;
using Funcky.Monads;

namespace Messerli.FileSystem
{
    public interface IHierarchyLocator
    {
        /// <summary>Returns the closest parent directory, starting with the <paramref name="startingDirectory"/>
        /// containing a file with the given <paramref name="fileName"/>.</summary>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fileName"/> contains invalid characters or is empty.</exception>
        Option<string> FindClosestParentDirectoryContainingFile(string fileName, string startingDirectory);
    }
}
