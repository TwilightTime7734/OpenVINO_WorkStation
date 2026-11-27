using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WorkStationC_;

internal sealed class ChatTab(OvmsChat chat, WebTools web, Func<Uri> address, ComboBox model, RichTextBox transcript,
    TextBox input, Label images, Label folder, Button send, CheckBox useTools, CheckBox runCommandsWithoutAsking,
    Func<int?> contextTokens, Action<string> output)
{
    // Conversation size kept below 60% of what the model's reserved working memory holds (room for the
    // reply, tool results, and instructions), and at most 32,000 tokens so each message stays quick.
    private const int DefaultBudget = 24_000;
    private const int MaxBudget = 32_000;

    private int TokenBudget() => contextTokens() is { } tokens
        ? Math.Clamp(tokens / 10 * 6, 4_000, MaxBudget)
        : DefaultBudget;

    private readonly List<string> _images = [];
    private readonly StringBuilder _pendingReply = new();
    private readonly Stopwatch _lastReplyDraw = Stopwatch.StartNew();
    private readonly FileTools _files = new(question => AskPermission(transcript, question));
    private ChatTools Tools => new(web, _files,
        new CommandTool(_files, question => AskPermission(transcript, question), () => runCommandsWithoutAsking.Checked));

    public bool IsReplying { get; private set; }

    public void ShowToolStatus() => output(web.CanSearch
        ? $"Chat web tools: search with {web.Providers} (keys from {WebTools.KeyFileName}) and read web pages."
        : $"Chat web tools: no search keys found in {WebTools.KeyFileName}; only page reading is available.");

    public void ChooseFolder()
    {
        using FolderBrowserDialog dialog = new()
        {
            Description = "Choose the working folder. The model can read and write files here and in its subfolders without asking.",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true,
            SelectedPath = _files.Root ?? string.Empty,
        };
        if (dialog.ShowDialog(transcript.FindForm()) != DialogResult.OK) return;
        _files.Root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(dialog.SelectedPath));
        // The folder path is only known at runtime.
        folder.Text = $"Working folder: {_files.Root}";
        output($"Chat working folder: {_files.Root}. The model can list, read, and write files there and in its subfolders. Anything else asks you first.");
    }

    public void ClearFolder()
    {
        _files.Root = null;
        folder.Text = "Working folder: none (the model cannot use your files)";
        output("Chat working folder cleared.");
    }

    // Yes allows this one action, No refuses it, and Cancel stops the whole reply.
    private static bool AskPermission(Control owner, string question)
    {
        if (owner.InvokeRequired) return (bool)owner.Invoke(AskPermission, owner, question);
        var answer = MessageBox.Show(owner.FindForm(),
            $"{question}\n\nYes = allow   No = refuse this one   Cancel = stop the reply",
            "Allow this?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        if (answer == DialogResult.Cancel) throw new OperationCanceledException("The user stopped the reply.");
        return answer == DialogResult.Yes;
    }

    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        string previous = model.Text;
        string[] models;
        try { models = await OvmsChat.ListModelsAsync(address(), cancellationToken); }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            model.Items.Clear();
            output($"No OVMS server is answering at {address()}. Load a model on Workstation first.");
            return;
        }
        model.Items.Clear();
        model.Items.AddRange(models);
        if (models.Length == 0) { output("The server is running but has no models yet."); return; }
        model.SelectedIndex = Math.Max(0, model.Items.IndexOf(previous));
        output($"Chat models: {string.Join(", ", models)}");
    }

    public async Task SendAsync(CancellationToken cancellationToken)
    {
        string text = input.Text.Trim();
        if (text.Length == 0) return;
        if (model.Items.Count == 0) await RefreshAsync(cancellationToken);
        if (model.Text.Length == 0) throw new InvalidOperationException("Choose a model on the Chat tab first.");
        string[] attached = [.. _images];
        Append($"You: {text}{(attached.Length > 0 ? $"  [{attached.Length} image(s)]" : string.Empty)}{Environment.NewLine}", Color.SteelBlue);
        input.Clear();
        ClearImages();
        Append($"{model.Text}: ", Color.DarkGreen);
        // While a reply is coming in, the Send button stops it. The text changes so that is obvious.
        IsReplying = true;
        send.Text = "Stop";
        try
        {
            chat.TokenBudget = TokenBudget();
            await chat.SendAsync(address(), model.Text, text, attached, useTools.Checked ? Tools : null,
                AppendReply, AppendToolNote, cancellationToken);
            if (chat.LastDroppedExchanges > 0)
                output($"Removed the {chat.LastDroppedExchanges} oldest exchange(s) to keep the conversation within the model's working memory.");
            int tokens = chat.ConversationCharacters / 4;
            output($"Conversation size: about {tokens:N0} of {chat.TokenBudget:N0} tokens allowed; older exchanges are removed past that.");
        }
        catch (OperationCanceledException)
        {
            FlushReply();
            Append($"{Environment.NewLine}(stopped)", Color.Gray);
            throw;
        }
        catch (RepeatingReplyException ex)
        {
            FlushReply();
            Append($"{Environment.NewLine}({ex.Message})", Color.DarkOrange);
            output(ex.Message);
            return;
        }
        catch (Exception ex)
        {
            FlushReply();
            Append($"{Environment.NewLine}(error: {ex.Message})", Color.Firebrick);
            throw;
        }
        finally
        {
            FlushReply();
            Append(Environment.NewLine + Environment.NewLine, transcript.ForeColor);
            send.Text = "Send Message";
            IsReplying = false;
        }
    }

    public void AttachImages()
    {
        using OpenFileDialog dialog = new()
        {
            Title = "Choose images to send with your next message",
            Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp",
            Multiselect = true,
        };
        if (dialog.ShowDialog(transcript.FindForm()) != DialogResult.OK) return;
        _images.AddRange(dialog.FileNames);
        images.Text = $"Images for next message: {string.Join(", ", _images.Select(Path.GetFileName))}";
    }

    public void NewChat()
    {
        chat.Clear();
        transcript.Clear();
        ClearImages();
    }

    private void ClearImages()
    {
        _images.Clear();
        images.Text = "No images attached.";
    }

    // Replies arrive a few characters at a time. Collect them and draw about ten times a second
    // so the transcript stays still enough to read.
    private void AppendReply(string text)
    {
        _pendingReply.Append(text);
        if (_lastReplyDraw.ElapsedMilliseconds >= 100) FlushReply();
    }

    private void AppendToolNote(string note)
    {
        FlushReply();
        Append($"{Environment.NewLine}[{note}]{Environment.NewLine}", Color.Gray);
    }

    private void FlushReply()
    {
        _lastReplyDraw.Restart();
        if (_pendingReply.Length == 0) return;
        string text = _pendingReply.ToString();
        _pendingReply.Clear();
        Append(text, transcript.ForeColor);
    }

    // Adds text at the end without moving the reader: follows new text only when already at the bottom,
    // keeps the scroll position and any selection otherwise, and draws once per call.
    private void Append(string text, Color color)
    {
        if (transcript.InvokeRequired) { transcript.BeginInvoke(Append, text, color); return; }
        IntPtr handle = transcript.Handle;
        bool followEnd = IsScrolledToBottom(handle);
        Point scroll = default;
        SendMessage(handle, EM_GETSCROLLPOS, IntPtr.Zero, ref scroll);
        int selectionStart = transcript.SelectionStart;
        int selectionLength = transcript.SelectionLength;
        SendMessage(handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
        try
        {
            transcript.Select(transcript.TextLength, 0);
            transcript.SelectionColor = color;
            transcript.SelectedText = text;
            transcript.Select(selectionStart, selectionLength);
            if (followEnd) SendMessage(handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
            else SendMessage(handle, EM_SETSCROLLPOS, IntPtr.Zero, ref scroll);
        }
        finally
        {
            SendMessage(handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            transcript.Invalidate();
        }
    }

    private static bool IsScrolledToBottom(IntPtr handle)
    {
        SCROLLINFO info = new() { cbSize = (uint)Marshal.SizeOf<SCROLLINFO>(), fMask = SIF_ALL };
        if (!GetScrollInfo(handle, SB_VERT, ref info)) return true;
        return info.nPage == 0 || info.nPos + (int)info.nPage >= info.nMax - 2;
    }

    private const int WM_SETREDRAW = 0x000B;
    private const int WM_VSCROLL = 0x0115;
    private const int SB_BOTTOM = 7;
    private const int SB_VERT = 1;
    private const uint SIF_ALL = 0x17;
    private const int EM_GETSCROLLPOS = 0x0400 + 221;
    private const int EM_SETSCROLLPOS = 0x0400 + 222;

    [StructLayout(LayoutKind.Sequential)]
    private struct SCROLLINFO
    {
        public uint cbSize;
        public uint fMask;
        public int nMin;
        public int nMax;
        public uint nPage;
        public int nPos;
        public int nTrackPos;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref Point lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetScrollInfo(IntPtr hWnd, int nBar, ref SCROLLINFO info);
}
