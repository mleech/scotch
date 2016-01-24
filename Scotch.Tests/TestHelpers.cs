using System.IO;
using System.Runtime.CompilerServices;

namespace Scotch.Tests
{
    public static class TestHelpers
    {

        public static string GetSourceFileDirectory([CallerFilePath] string sourceFilePath = "")
        {
            return Path.GetDirectoryName(sourceFilePath);
        }

    }
}
