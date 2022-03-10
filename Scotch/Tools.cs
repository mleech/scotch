using System.Runtime.CompilerServices;

namespace Scotch;

public static class Tools
{
    public static string GetFilePathInCurrentDirectory(string fileName)
    {
        return Path.Combine(GetSourceFileDirectory(), fileName);
    }

    private static string GetSourceFileDirectory([CallerFilePath] string sourceFilePath = "")
    {
        if (string.IsNullOrEmpty(sourceFilePath)) throw new ArgumentNullException(nameof(sourceFilePath));

        var path = Path.GetDirectoryName(sourceFilePath);
        if (string.IsNullOrEmpty(path)) throw new ArgumentException("Could not get directory from source file path");

        return path;
    }
}
