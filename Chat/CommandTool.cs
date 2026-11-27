using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;

namespace WorkStationC_;

// Lets the chat model run a command in the working folder. A command is not limited to that folder,
// so each one is shown to the user for a Yes/No first unless "Run commands without asking" is on.
internal sealed class CommandTool(FileTools files, Func<string, bool> askPermission, Func<bool> runWithoutAsking)
{
    public const string Name = "run_command";
    private const int DefaultTimeoutSeconds = 60;
    private const int MaxTimeoutSeconds = 300;
    // Output goes back to the model and stays in the conversation: keep the start and the end (where errors and results are).
    private const int HeadCharacters = 2_000;
    private const int TailCharacters = 6_000;

    public JsonArray Definitions()
    {
        if (files.Root is null) return [];
        return
        [
            WebTools.Function(Name,
                "Run a Windows command in the working folder and return its output and exit code. " +
                "Use it to build, test, run scripts, or inspect the system. Commands that wait for keyboard input will time out.",
                new JsonObject
                {
                    ["command"] = new JsonObject { ["type"] = "string", ["description"] = "The command line to run." },
                    ["shell"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("cmd", "powershell"), ["description"] = "cmd (default) or powershell." },
                    ["timeout_seconds"] = new JsonObject { ["type"] = "integer", ["description"] = $"Stop the command after this many seconds. Default {DefaultTimeoutSeconds}, at most {MaxTimeoutSeconds}." },
                }, "command"),
        ];
    }

    public static string Describe(JsonObject arguments) => $"Running command: {Text(arguments, "command")}";

    public async Task<string> RunAsync(JsonObject arguments, CancellationToken cancellationToken)
    {
        string command = Text(arguments, "command");
        if (command.Length == 0) return "Error: the command is empty.";
        if (files.Root is null || !Directory.Exists(files.Root)) return "Error: choose a working folder on the Chat tab before running commands.";
        bool powershell = Text(arguments, "shell").Equals("powershell", StringComparison.OrdinalIgnoreCase);
        int timeout = int.TryParse(arguments["timeout_seconds"]?.ToString(), out int seconds) ? Math.Clamp(seconds, 1, MaxTimeoutSeconds) : DefaultTimeoutSeconds;

        if (!runWithoutAsking() && !askPermission(
            $"The model wants to run this {(powershell ? "PowerShell" : "cmd")} command:\n\n{command}\n\nin {files.Root}\n\n" +
            "A command can change anything on this PC, not only the working folder. Run it?"))
            return "Error: the user did not allow this command.";

        ProcessStartInfo info = powershell
            ? new("powershell.exe") { ArgumentList = { "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-Command", command } }
            : new("cmd.exe") { Arguments = $"/d /s /c \"{command}\"" };
        info.WorkingDirectory = files.Root;
        info.UseShellExecute = false;
        info.CreateNoWindow = true;
        info.RedirectStandardInput = true;
        info.RedirectStandardOutput = true;
        info.RedirectStandardError = true;
        info.StandardOutputEncoding = Encoding.UTF8;
        info.StandardErrorEncoding = Encoding.UTF8;

        using Process process = new() { StartInfo = info };
        StringBuilder head = new();
        Queue<string> tail = new();
        int tailLength = 0;
        long omitted = 0;
        object gate = new();
        void Collect(string? line)
        {
            if (line is null) return;
            lock (gate)
            {
                if (head.Length + line.Length < HeadCharacters) { head.AppendLine(line); return; }
                tail.Enqueue(line);
                tailLength += line.Length + 1;
                while (tailLength > TailCharacters && tail.Count > 1)
                {
                    string dropped = tail.Dequeue();
                    tailLength -= dropped.Length + 1;
                    omitted += dropped.Length + 1;
                }
            }
        }
        process.OutputDataReceived += (_, e) => Collect(e.Data);
        process.ErrorDataReceived += (_, e) => Collect(e.Data);
        process.Start();
        process.StandardInput.Close();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var limit = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limit.CancelAfter(TimeSpan.FromSeconds(timeout));
        bool timedOut = false;
        try { await process.WaitForExitAsync(limit.Token); }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
            cancellationToken.ThrowIfCancellationRequested();
            timedOut = true;
        }
        if (!timedOut) process.WaitForExit(); // Let the output readers finish.

        string text;
        lock (gate)
        {
            string last = string.Join("\n", tail);
            if (last.Length > TailCharacters) last = last[^TailCharacters..];
            text = (head.ToString() + (omitted > 0 ? $"[... {omitted:N0} characters of output skipped ...]\n" : string.Empty) + last).TrimEnd();
        }
        if (text.Length == 0) text = "(no output)";
        return timedOut
            ? $"The command was stopped after {timeout} seconds.\n{text}"
            : $"Exit code {process.ExitCode}.\n{text}";
    }

    private static string Text(JsonObject arguments, string name) => arguments[name]?.ToString().Trim() ?? string.Empty;
}
