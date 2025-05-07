using System;
using System.IO;

namespace Nexus.Logging.Harness.IntegrationTests;

public static class Utilities
{
    public static string GetSolutionDirectory()
    {
        var current = AppContext.BaseDirectory;
        var di = new DirectoryInfo(current);

        while (di != null && !di.Name.Equals("TestHarness", StringComparison.OrdinalIgnoreCase)) di = di.Parent;

        return di.FullName;
    }
}