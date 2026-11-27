using System.Text.Json.Nodes;

namespace WorkStationC_;

// The tools offered to the chat model: web tools always; file and command tools when a working folder is chosen.
internal sealed class ChatTools(WebTools web, FileTools files, CommandTool commands)
{
    public JsonArray Definitions()
    {
        JsonArray tools = web.Definitions();
        foreach (var tool in files.Definitions()) tools.Add(tool!.DeepClone());
        foreach (var tool in commands.Definitions()) tools.Add(tool!.DeepClone());
        return tools;
    }

    public string Instructions()
    {
        string text = $"Today's date is {DateTime.Now:dddd, MMMM d, yyyy}. The computer runs Windows. You can use tools. " +
            (web.CanSearch ? "web_search finds current information on the web and " : string.Empty) +
            "read_web_page reads a web page; use them when a question needs current, local, or specific facts you are not sure of, " +
            "then answer plainly and list the links you relied on.";
        if (files.Root is not null)
            text += $" The user's working folder is {files.Root}. list_files, read_file, and write_file work there freely; " +
                "use paths relative to it. Paths outside it need the user's permission, so avoid them unless asked. " +
                $"{CommandTool.Name} runs a Windows command (cmd or powershell) in the working folder and returns its output; " +
                "the user may be asked to approve each command, so explain briefly why you need it.";
        else
            text += " There is no working folder, so file and command tools are off. If the user asks for them, tell them to press Working Folder on the Chat tab.";
        return text;
    }

    public static string Describe(string name, JsonObject arguments) =>
        name == CommandTool.Name ? CommandTool.Describe(arguments)
        : FileTools.Names.Contains(name) ? FileTools.Describe(name, arguments)
        : WebTools.Describe(name, arguments);

    public async Task<string> RunAsync(string name, JsonObject arguments, CancellationToken cancellationToken) =>
        name == CommandTool.Name ? await commands.RunAsync(arguments, cancellationToken)
        : FileTools.Names.Contains(name) ? files.Run(name, arguments)
        : await web.RunAsync(name, arguments, cancellationToken);
}
