using System.Text;
using System.Text.Json.Nodes;
using static WorkStationC_.AppFiles;

namespace WorkStationC_;

// File tools for the chat model. Paths inside the working folder (and its subfolders) are used without asking.
// Any other path, or a path that passes through a link or junction, needs the user's Yes first.
internal sealed class FileTools(Func<string, bool> askPermission)
{
    // Every tool result is sent back to the model and kept in the conversation, so keep each one modest.
    private const int MaxReadCharacters = 20_000;
    private const int DefaultReadLines = 400;
    private const int MaxListEntries = 200;
    private static readonly HashSet<string> SkippedFolders = new(StringComparer.OrdinalIgnoreCase)
        { ".git", ".vs", "bin", "obj", "node_modules", "__pycache__", ".venv", "venv" };

    public string? Root { get; set; }

    public static readonly string[] Names = ["list_files", "read_file", "write_file"];

    public JsonArray Definitions()
    {
        if (Root is null) return [];
        return
        [
            WebTools.Function("list_files", $"List files and folders in the working folder or one of its subfolders (at most {MaxListEntries}; build folders such as bin, obj, and .git are skipped when recursive).",
                new JsonObject
                {
                    ["path"] = new JsonObject { ["type"] = "string", ["description"] = "Folder relative to the working folder. Use \".\" for the working folder itself." },
                    ["recursive"] = new JsonObject { ["type"] = "boolean", ["description"] = "Also list everything in subfolders. Default false." },
                }, "path"),
            WebTools.Function("read_file", $"Read part of a text file: up to {DefaultReadLines} lines or {MaxReadCharacters:N0} characters at a time. " +
                "The result says how many lines the file has; use start_line to read further. Read only what you need.",
                new JsonObject
                {
                    ["path"] = new JsonObject { ["type"] = "string", ["description"] = "File path relative to the working folder." },
                    ["start_line"] = new JsonObject { ["type"] = "integer", ["description"] = "First line to read, starting at 1. Default 1." },
                    ["max_lines"] = new JsonObject { ["type"] = "integer", ["description"] = $"How many lines to read. Default and maximum {DefaultReadLines}." },
                }, "path"),
            WebTools.Function("write_file", "Create or replace a text file. Missing folders are created.",
                new JsonObject
                {
                    ["path"] = new JsonObject { ["type"] = "string", ["description"] = "File path relative to the working folder." },
                    ["content"] = new JsonObject { ["type"] = "string", ["description"] = "The full text to write." },
                }, "path"),
        ];
    }

    public static string Describe(string name, JsonObject arguments) => name switch
    {
        "list_files" => $"Listing files: {Text(arguments, "path")}",
        "read_file" => $"Reading file: {Text(arguments, "path")}",
        "write_file" => $"Writing file: {Text(arguments, "path")}",
        _ => $"Using tool {name}",
    };

    public string Run(string name, JsonObject arguments)
    {
        try
        {
            return name switch
            {
                "list_files" => List(Resolve(Text(arguments, "path"), "list the files in"), arguments["recursive"]?.ToString() is "true" or "True"),
                "read_file" => Read(Resolve(Text(arguments, "path"), "read"), Number(arguments, "start_line", 1), Number(arguments, "max_lines", DefaultReadLines)),
                "write_file" => Write(Resolve(Text(arguments, "path"), "write"), arguments["content"]?.ToString() ?? string.Empty),
                _ => $"Error: there is no tool named {name}.",
            };
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return $"Error: {ex.Message}";
        }
    }

    // Returns the full path when it is inside the working folder, or when the user allows it.
    internal string Resolve(string path, string action)
    {
        if (path.Length == 0) path = ".";
        string full = Root is not null && !Path.IsPathRooted(path) ? Path.GetFullPath(Path.Combine(Root, path)) : Path.GetFullPath(path);
        if (IsInsideRoot(full)) return full;
        string where = Root is null ? "No working folder is chosen." : $"This is outside the working folder:\n{Root}";
        if (askPermission($"The model wants to {action}:\n{full}\n\n{where}\n\nAllow this one time?")) return full;
        throw new UnauthorizedAccessException($"The user did not allow access to {full}. Only the working folder {Root ?? "(none)"} can be used freely.");
    }

    private bool IsInsideRoot(string full)
    {
        if (Root is null || !IsPathUnder(full, Root)) return false;
        // A link or junction inside the folder could point anywhere, so treat it as outside.
        string root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Root));
        for (string? current = full; current is not null && current.Length > root.Length; current = Path.GetDirectoryName(current))
        {
            FileSystemInfo info = Directory.Exists(current) ? new DirectoryInfo(current) : new FileInfo(current);
            if (info.Exists && info.Attributes.HasFlag(FileAttributes.ReparsePoint)) return false;
        }
        return true;
    }

    private string List(string folder, bool recursive)
    {
        if (!Directory.Exists(folder)) return $"Error: the folder does not exist: {folder}";
        StringBuilder text = new($"Contents of {folder}:\n");
        int count = 0;
        Queue<string> folders = new([folder]);
        while (folders.Count > 0 && count <= MaxListEntries)
        {
            string current = folders.Dequeue();
            var options = new EnumerationOptions { IgnoreInaccessible = true, AttributesToSkip = FileAttributes.ReparsePoint };
            foreach (string entry in Directory.EnumerateFileSystemEntries(current, "*", options).Order(StringComparer.OrdinalIgnoreCase))
            {
                if (++count > MaxListEntries) { text.Append($"[stopped after {MaxListEntries} entries; list a subfolder to see more]\n"); break; }
                string relative = Path.GetRelativePath(folder, entry);
                if (!Directory.Exists(entry))
                {
                    text.Append($"{relative}  ({FormatBytes((ulong)new FileInfo(entry).Length)})\n");
                    continue;
                }
                bool skipped = recursive && SkippedFolders.Contains(Path.GetFileName(entry));
                text.Append($"{relative}{Path.DirectorySeparatorChar}{(skipped ? "  [not listed]" : string.Empty)}\n");
                if (recursive && !skipped) folders.Enqueue(entry);
            }
        }
        if (count == 0) text.Append("(empty)\n");
        return text.ToString();
    }

    private static string Read(string file, int startLine, int maxLines)
    {
        if (!File.Exists(file)) return $"Error: the file does not exist: {file}";
        startLine = Math.Max(1, startLine);
        maxLines = Math.Clamp(maxLines, 1, DefaultReadLines);
        StringBuilder text = new();
        int lineNumber = 0, lastLine = 0;
        bool cutShort = false;
        using var reader = new StreamReader(file, detectEncodingFromByteOrderMarks: true);
        while (reader.ReadLine() is { } line)
        {
            lineNumber++;
            if (lineNumber < startLine || cutShort || lineNumber >= startLine + maxLines) continue;
            if (line.Contains('\0')) return "Error: this looks like a binary file, not text.";
            if (text.Length == 0 && line.Length > MaxReadCharacters)
            {
                // One very long line (for example minified code): show its start only.
                text.AppendLine(line[..MaxReadCharacters] + " [line shortened]");
                lastLine = lineNumber;
                cutShort = true;
                continue;
            }
            if (text.Length + line.Length > MaxReadCharacters) { cutShort = true; continue; }
            text.AppendLine(line);
            lastLine = lineNumber;
        }
        if (lineNumber == 0) return $"{file} is empty.";
        if (startLine > lineNumber) return $"Error: {file} has only {lineNumber} lines.";
        string header = lastLine == lineNumber && startLine == 1 ? $"{file} ({lineNumber} lines):\n"
            : lastLine == lineNumber ? $"{file}, lines {startLine}-{lastLine} of {lineNumber} (end of file).\n"
            : $"{file}, lines {startLine}-{lastLine} of {lineNumber}. Use start_line={lastLine + 1} to read more.\n";
        return header + text;
    }

    private static int Number(JsonObject arguments, string name, int fallback) =>
        int.TryParse(arguments[name]?.ToString(), out int value) ? value : fallback;

    private static string Write(string file, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        bool existed = File.Exists(file);
        File.WriteAllText(file, content);
        return $"{(existed ? "Replaced" : "Created")} {file} ({FormatBytes((ulong)new FileInfo(file).Length)}).";
    }

    private static string Text(JsonObject arguments, string name) => arguments[name]?.ToString().Trim() ?? string.Empty;
}
