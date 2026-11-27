
using System.Runtime.InteropServices;
using static WorkStationC_.AppFiles;

namespace WorkStationC_;

internal static class OpenVinoRuntime
{
    internal static string Configure()
    {
        string[] candidates =
        [
            Path.Combine(AppContext.BaseDirectory, "runtimes", "win-x64", "native"),
            Path.Combine(AppContext.BaseDirectory, "openvino", "runtime", "bin", "intel64", "Release"),
            Path.Combine(AppContext.BaseDirectory, "openvino"),
            Path.Combine(GetProjectDirectory(), "Runtime", "OpenVINO", "win-x64", "native"),
            Environment.GetEnvironmentVariable("OPENVINO_RUNTIME_DIR") ?? string.Empty,
        ];
        foreach (string candidate in candidates.Where(Directory.Exists))
        {
            string[] folders = [candidate, Path.Combine(candidate, "bin"),
                Path.Combine(candidate, "runtimes", "win-x64", "native"),
                Path.Combine(candidate, "runtime", "bin", "intel64", "Release")];
            string? folder = folders.FirstOrDefault(path => File.Exists(Path.Combine(path, "openvino_c.dll")));
            if (folder is null) continue;
            Environment.SetEnvironmentVariable("OPENVINO_RUNTIME_DIR", folder);
            string path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            if (!path.Split(Path.PathSeparator).Contains(folder, StringComparer.OrdinalIgnoreCase))
                Environment.SetEnvironmentVariable("PATH", folder + Path.PathSeparator + path);
            SetDllDirectory(folder);
            return folder;
        }
        throw new DirectoryNotFoundException("OpenVINO runtime DLLs were not found. Build the project to restore the bundled runtime.");
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SetDllDirectory(string path);
}
