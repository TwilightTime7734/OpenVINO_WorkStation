using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using WorkStationC_;

internal static class Checks
{
    private static int _passed;
    private static bool _render;

    [STAThread]
    private static void Main(string[] args)
    {
        _render = args.Contains("--render");
        ApplicationConfiguration.Initialize();
        using ApplicationContext context = new();
        SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
        SynchronizationContext.Current!.Post(async _ =>
        {
            try
            {
                await RunAsync();
                if (args.Length == 2) await CheckChatModelAsync(args[0], args[1]);
                Console.WriteLine($"PASS: {_passed} checks");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                Environment.ExitCode = 1;
            }
            finally { context.ExitThread(); }
        }, null);
        Application.Run(context);
    }

    private static void Check(bool condition, string description)
    {
        if (!condition) throw new Exception(description);
        _passed++;
        Console.WriteLine("PASS: " + description);
    }

    private static async Task RunAsync()
    {
        string modelRoot = AppFiles.GetModelRoot();
        string testRoot = Path.Combine(modelRoot, ".workstation-checks-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testRoot);
        try
        {
            Check(!AppFiles.IsPathUnder(modelRoot + "-other/file.bin", modelRoot), "Sibling paths are outside the model root");
            var settings = OvmsPull.LoadSettings();
            Check(settings.SourceModels.Count == 4 && settings.Commands.Count == 4, "All four OVMS model presets survive");
            Check(settings.Versions.Count == 7 && settings.PipelineType == "VLM", "OVMS version expectations and selected pipeline survive");
            var gguf = OvmsPull.BuildOvmsPullArguments("text_generation", "LM", "owner/model", "model.gguf", testRoot, "int4", "", false, false, true);
            Check(gguf.Contains("--gguf_filename") && !gguf.Contains("--weight-format"), "GGUF pull uses direct-download arguments");
            var export = OvmsPull.BuildOvmsPullArguments("text_generation", "LM", "owner/model", "", testRoot, "int4", "", false, false, false);
            Check(export.Contains("--pipeline_type") && export.Contains("--weight-format") && !export.Contains("--gguf_filename"), "Model export uses pipeline and weight arguments");

            string diffusion = Path.Combine(testRoot, "diffusion");
            Directory.CreateDirectory(diffusion);
            File.WriteAllText(Path.Combine(diffusion, "model_index.json"), "{\"_class_name\":\"StableDiffusionPipeline\"}");
            Check(ModelFiles.ClassifyModelRunPath(diffusion, diffusion, ModelFileKind.OpenVinoGenAiDirectory).RunPath == ModelRunPath.TextToImage,
                "Image-generation models are not classified as chat models");
            string vision = Path.Combine(testRoot, "vision");
            Directory.CreateDirectory(vision);
            File.WriteAllText(Path.Combine(vision, "config.json"), "{\"model_type\":\"qwen2_vl\",\"vision_config\":{}}");
            Check(ModelFiles.ClassifyModelRunPath(vision, vision, ModelFileKind.OpenVinoGenAiDirectory).RunPath == ModelRunPath.VisionLanguage,
                "Vision models retain their own loading path");

            await CheckDownloadsAsync(testRoot);
            await CheckDownloadAddressesAsync();
            await CheckServerAsync();
            await CheckLiveStreamingAsync();
            await ToolCallingChecks.RunAsync();
            await CheckImagesAsync();
            await CheckNativeModelReadAsync(testRoot);
            await CheckFormAsync(Path.Combine(testRoot, "relu.xml"));
        }
        finally
        {
            // Delete only this run's uniquely named test directory, never the user's model root.
            string fullPath = Path.GetFullPath(testRoot);
            if (AppFiles.IsPathUnder(fullPath, modelRoot) && Path.GetFileName(fullPath).StartsWith(".workstation-checks-"))
                Directory.Delete(fullPath, recursive: true);
        }
    }

    private static async Task CheckDownloadsAsync(string root)
    {
        using HttpClient client = new(new DownloadResponses());
        string path = Path.Combine(root, "test.gguf");
        var downloaded = await FileDownload.DownloadAsync(client, new("https://fixture/valid"), path, "test", (_, _, _, _) => { }, CancellationToken.None);
        Check(!downloaded.Skipped && File.ReadAllText(path) == "GGUFtest", "Download bytes reach the final file");
        var skipped = await FileDownload.DownloadAsync(client, new("https://fixture/valid"), path, "test", (_, _, _, _) => { }, CancellationToken.None);
        Check(skipped.Skipped, "A complete download is skipped");
        string badPath = Path.Combine(root, "invalid.gguf");
        try
        {
            await FileDownload.DownloadAsync(client, new("https://fixture/invalid"), badPath, "test", (_, _, _, _) => { }, CancellationToken.None);
            throw new Exception("Invalid GGUF was accepted");
        }
        catch (InvalidDataException) { }
        Check(!File.Exists(badPath) && !File.Exists(badPath + ".partial"), "An invalid model leaves no completed or partial file");

        using CancellationTokenSource cancellation = new();
        string cancelPath = Path.Combine(root, "cancel.bin");
        try
        {
            await FileDownload.DownloadAsync(client, new("https://fixture/slow"), cancelPath, "test",
                (_, _, _, _) => cancellation.Cancel(), cancellation.Token);
            throw new Exception("Download cancellation did not propagate");
        }
        catch (OperationCanceledException) { }
        Check(!File.Exists(cancelPath) && !File.Exists(cancelPath + ".partial"), "Cancel removes an interrupted download");
    }

    private static async Task CheckServerAsync()
    {
        using TcpListener portProbe = new(IPAddress.Loopback, 0);
        portProbe.Start();
        int port = ((IPEndPoint)portProbe.LocalEndpoint).Port;
        portProbe.Stop();
        LocalModelApiTextRequest? received = null;
        TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        bool requestCanceled = false;
        using LocalModelApiServer server = new(
            () => new LocalModelApiSnapshot { Models = [new() { Name = "fixture", Source = "test", RuntimePath = "test", SupportsChat = true }] },
            async (request, token) =>
            {
                received = request;
                if (request.Prompt == "wait")
                {
                    started.SetResult();
                    try { await Task.Delay(Timeout.Infinite, token); }
                    catch (OperationCanceledException) { requestCanceled = true; throw; }
                }
                return LocalModelApiTextResult.Ok("hello", "fixture", 0, 1);
            },
            Console.WriteLine);
        server.Start(IPAddress.Loopback, port);
        using HttpClient client = new() { BaseAddress = new Uri(server.BaseUrl), Timeout = TimeSpan.FromSeconds(5) };
        Check((await client.GetAsync("/health")).IsSuccessStatusCode, "Local server health endpoint responds");
        foreach (string route in new[] { "/v2/health/live", "/v2/health/ready" })
        {
            using var health = await client.GetAsync(route);
            Check(health.StatusCode == HttpStatusCode.OK && (await health.Content.ReadAsByteArrayAsync()).Length == 0,
                $"OVMS {route} returns status without a response body");
        }
        using var metadata = JsonDocument.Parse(await client.GetStringAsync("/health"));
        Check(metadata.RootElement.GetProperty("api_base_url").GetString() == server.BaseUrl + "/v3",
            "Server metadata advertises the OVMS API base URL");
        string models = await client.GetStringAsync("/v1/models");
        Check(models.Contains("fixture"), "Local server lists available models");
        var response = await client.PostAsJsonAsync("/v1/chat/completions", new { model = "fixture", messages = new[] { new { role = "user", content = "test question" } }, max_tokens = 23 });
        string chat = await response.Content.ReadAsStringAsync();
        Check(response.IsSuccessStatusCode && chat.Contains("hello") && received?.Prompt == "test question" && received.MaxTokens == 23,
            "Chat request parameters and response survive the server round trip");
        string imageUrl = TestImageUrl();
        var imageResponse = await client.PostAsJsonAsync("/v3/chat/completions", new { messages = new[] { new { role = "user",
            content = new object[] { new { type = "text", text = "Describe this" }, new { type = "image_url", image_url = new { url = imageUrl } } } } } });
        Check(imageResponse.IsSuccessStatusCode && received?.ImageUrls.SequenceEqual(new[] { imageUrl }) == true
            && received.Prompt.Contains("<ov_genai_image_0>"), "Open WebUI image attachments reach generation with ordered image markers");
        var imageOnly = await client.PostAsJsonAsync("/v3/chat/completions", new { messages = new[] { new { role = "user",
            content = new[] { new { type = "image_url", image_url = new { url = imageUrl } } } } } });
        Check(imageOnly.IsSuccessStatusCode && received?.ImageUrls.Count == 1, "Image-only chat messages are accepted");
        var stream = await client.PostAsJsonAsync("/v1/completions", new { model = "fixture", prompt = "hello", stream = true });
        Check((await stream.Content.ReadAsStringAsync()).Contains("[DONE]"), "Streaming responses terminate correctly");
        Check((await client.PostAsync("/v1/chat/completions", new StringContent("{", Encoding.UTF8, "application/json"))).StatusCode == HttpStatusCode.BadRequest,
            "Malformed chat JSON produces an HTTP error");
        Check((await client.PostAsync("/v1/images/generations", new StringContent("{}"))).StatusCode == HttpStatusCode.NotImplemented,
            "Unsupported image generation still returns HTTP 501");
        Check((await client.GetAsync("/missing")).StatusCode == HttpStatusCode.NotFound, "Unknown API routes return HTTP 404");
        var preflight = await client.SendAsync(new HttpRequestMessage(HttpMethod.Options, "/v1/chat/completions"));
        Check(preflight.StatusCode == HttpStatusCode.NoContent && preflight.Headers.Contains("Access-Control-Allow-Origin"), "Browser preflight requests retain CORS support");
        var oversized = await client.PostAsync("/v1/chat/completions", new StringContent(new string(' ', LocalModelApiServer.MaxRequestBodyBytes + 1)));
        Check(oversized.StatusCode == HttpStatusCode.RequestEntityTooLarge, "The HTTP request size limit remains enforced");
        var pending = client.PostAsJsonAsync("/v1/completions", new { model = "fixture", prompt = "wait" });
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await server.StopAsync();
        try { await pending; } catch (HttpRequestException) { }
        Check(requestCanceled, "Stopping the server cancels active model generation");
        Check(!server.IsRunning, "The local server stops");
        server.Start(IPAddress.Loopback, port);
        Check((await client.GetAsync("/health")).IsSuccessStatusCode, "The server can restart on the same port");
        await server.StopAsync();
    }

    private static async Task CheckLiveStreamingAsync()
    {
        using TcpListener probe = new(IPAddress.Loopback, 0);
        probe.Start();
        int port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource canceled = new(TaskCreationOptions.RunContinuationsAsynchronously);
        LocalModelApiTextRequest? received = null;
        using LocalModelApiServer server = new(
            () => new() { ModelLoaded = true, LoadedModelName = "fixture", Models = [new() { Name = "fixture", Source = "test", RuntimePath = "test", SupportsChat = true }] },
            async (request, token) =>
            {
                received = request;
                if (request.Prompt == "reject") return LocalModelApiTextResult.Error(404, "Unknown model", "model_not_found");
                if (request.OnTextChunk is { } write)
                {
                    await write("Hello ");
                    if (request.Prompt == "fail") throw new InvalidOperationException("Generation failed after first token");
                    try { await release.Task.WaitAsync(token); }
                    catch (OperationCanceledException) { canceled.TrySetResult(); throw; }
                    await write("世界🙂\n");
                }
                return LocalModelApiTextResult.Ok("Hello 世界🙂\n", "fixture", 0, 1, 12, 4, "length");
            }, Console.WriteLine);
        server.Start(IPAddress.Loopback, port);
        using HttpClient client = new() { BaseAddress = new(server.BaseUrl), Timeout = TimeSpan.FromSeconds(10) };
        Check((await client.GetStringAsync("/v3/models")).Contains("fixture"), "OVMS /v3 model discovery works");
        Check((await client.GetStringAsync("/v3/models/fixture")).Contains("fixture"), "OVMS model retrieval works");
        Check((await client.GetAsync("/v3/models/missing")).StatusCode == HttpStatusCode.NotFound, "Unknown model retrieval returns 404");
        foreach (string route in new[] { "/v1/chat/completions", "/v3/chat/completions", "/v1/completions", "/v3/completions" })
        {
            release = new(TaskCreationOptions.RunContinuationsAsynchronously);
            bool chat = route.Contains("/chat/");
            using var message = new HttpRequestMessage(HttpMethod.Post, route)
            {
                Content = JsonContent.Create(new { model = "fixture", prompt = "hello", stream = true, max_tokens = 4096,
                    messages = new[] { new { role = "system", content = "Be helpful" }, new { role = "user", content = "hello" } },
                    chat_template_kwargs = new { enable_thinking = true }, stream_options = new { include_usage = true } })
            };
            using var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            Check(response.Content.Headers.ContentType?.MediaType == "text/event-stream" && response.Content.Headers.ContentLength is null,
                route + " uses SSE without a fixed content length");
            async Task<JsonDocument> ReadChunkAsync()
            {
                string? line = await reader.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(5));
                Check(line?.StartsWith("data: {") == true && await reader.ReadLineAsync() == "", "Each SSE event is separated by a blank line");
                return JsonDocument.Parse(line![6..]);
            }
            if (chat)
            {
                using var role = await ReadChunkAsync();
                Check(role.RootElement.GetProperty("choices")[0].GetProperty("delta").GetProperty("role").GetString() == "assistant", "Chat stream starts with the assistant role");
            }
            using var first = await ReadChunkAsync();
            var choice = first.RootElement.GetProperty("choices")[0];
            string? firstText = chat ? choice.GetProperty("delta").GetProperty("content").GetString() : choice.GetProperty("text").GetString();
            Check(firstText == "Hello " && !release.Task.IsCompleted && choice.GetProperty("finish_reason").ValueKind == JsonValueKind.Null,
                route + " delivers actual text before generation is allowed to finish");
            Check(received?.MaxTokens == 4096 && received.EnableThinking, "Token limit and OVMS chat template options reach generation");
            if (chat)
            {
                using var system = JsonDocument.Parse(received!.Messages[0]);
                Check(system.RootElement.GetProperty("role").GetString() == "system", "Chat history preserves system roles");
            }
            release.SetResult();
            string rest = await reader.ReadToEndAsync().WaitAsync(TimeSpan.FromSeconds(5));
            var events = rest.Split('\n').Where(line => line.StartsWith("data: ")).Select(line => line[6..]).ToArray();
            Check(events.Length == 4 && events[^1] == "[DONE]", "Stream sends content, finish, usage, and exactly one DONE");
            using var second = JsonDocument.Parse(events[0]);
            choice = second.RootElement.GetProperty("choices")[0];
            Check((chat ? choice.GetProperty("delta").GetProperty("content").GetString() : choice.GetProperty("text").GetString()) == "世界🙂\n",
                "Unicode and newline content survives SSE framing");
            Check(second.RootElement.GetProperty("id").GetString() == first.RootElement.GetProperty("id").GetString(), "Stream ID remains stable");
            using var finish = JsonDocument.Parse(events[1]);
            Check(finish.RootElement.GetProperty("choices")[0].GetProperty("finish_reason").GetString() == "length", "Token limit finish reason reaches the client");
            using var usage = JsonDocument.Parse(events[2]);
            Check(usage.RootElement.GetProperty("choices").GetArrayLength() == 0 && usage.RootElement.GetProperty("usage").GetProperty("total_tokens").GetInt32() == 16,
                "Requested stream usage is sent in an empty-choices final chunk");
        }
        var rejected = await client.PostAsJsonAsync("/v3/completions", new { prompt = "reject", stream = true });
        Check(rejected.StatusCode == HttpStatusCode.NotFound && rejected.Content.Headers.ContentType?.MediaType == "application/json",
            "Streaming validation errors keep their HTTP status before SSE starts");
        using (var message = new HttpRequestMessage(HttpMethod.Post, "/v3/completions") { Content = JsonContent.Create(new { prompt = "fail", stream = true }) })
        using (var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead))
        {
            bool aborted = false;
            try { await response.Content.ReadAsStringAsync(); }
            catch (HttpRequestException) { aborted = true; }
            Check(aborted, "A failed generation aborts the stream instead of reporting successful completion");
        }
        release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using (var message = new HttpRequestMessage(HttpMethod.Post, "/v3/completions") { Content = JsonContent.Create(new { prompt = "cancel", stream = true }) })
        {
            using var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
            var body = await response.Content.ReadAsStreamAsync();
            body.Dispose();
        }
        await canceled.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Check(true, "Disconnecting the streaming client cancels generation");
        await server.StopAsync();
    }

    private static string TestImageUrl()
    {
        using var bitmap = new Bitmap(3, 2);
        using (var graphics = Graphics.FromImage(bitmap)) graphics.Clear(Color.Red);
        bitmap.SetPixel(2, 1, Color.Blue);
        using var stream = new MemoryStream();
        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        return "data:image/png;base64," + Convert.ToBase64String(stream.ToArray());
    }

    private static async Task CheckImagesAsync()
    {
        OpenVinoRuntime.Configure();
        using var images = new ModelImages();
        await images.LoadAsync([TestImageUrl()], CancellationToken.None);
        byte[] rgb = images.Tensors[0].GetData<byte>(18);
        Check(rgb.Take(3).SequenceEqual(new byte[] { 255, 0, 0 }) && rgb.Skip(15).SequenceEqual(new byte[] { 0, 0, 255 }),
            "PNG images decode to RGB with correct row stride and pixel order");
        foreach (string invalid in new[] { "data:image/png;base64,bad!", "file:///C:/test.png", "data:image/png;base64,aGVsbG8=" })
        {
            bool rejected = false;
            try { await images.LoadAsync([invalid], CancellationToken.None); }
            catch (InvalidDataException) { rejected = true; }
            Check(rejected, "Invalid or unsupported image input is rejected");
        }
    }

    private static Task CheckNativeModelReadAsync(string root)
    {
        OpenVinoRuntime.Configure();
        string source = Path.Combine(root, "relu.xml");
        File.WriteAllText(source, """
            <net name="relu" version="11"><layers>
            <layer id="0" name="input" type="Parameter" version="opset1"><data shape="1,4" element_type="f32"/><output><port id="0" precision="FP32"><dim>1</dim><dim>4</dim></port></output></layer>
            <layer id="1" name="relu" type="Relu" version="opset1"><input><port id="0" precision="FP32"><dim>1</dim><dim>4</dim></port></input><output><port id="1" precision="FP32"><dim>1</dim><dim>4</dim></port></output></layer>
            <layer id="2" name="output" type="Result" version="opset1"><input><port id="0" precision="FP32"><dim>1</dim><dim>4</dim></port></input></layer>
            </layers><edges><edge from-layer="0" from-port="0" to-layer="1" to-port="0"/><edge from-layer="1" from-port="1" to-layer="2" to-port="0"/></edges></net>
            """);
        using OpenVinoSharp.Core core = new();
        using OpenVinoSharp.Model model = core.ReadModelUnicode(source);
        Check(model.Inputs.Count == 1 && model.Outputs.Count == 1, "OpenVINO IR models load directly through Core");
        return Task.CompletedTask;
    }

    private static async Task CheckChatModelAsync(string modelPath, string device)
    {
        Console.WriteLine($"Loading real chat model on {device}: {modelPath}");
        await using ModelSession model = new();
        await model.LoadAsync(modelPath, device, CancellationToken.None);
        string name = model.Snapshot().LoadedModelName;
        var first = await model.GenerateAsync(new() { Model = name, Prompt = "Say hello in one short sentence.", MaxTokens = 16 }, CancellationToken.None);
        Check(first.Success && first.LoadMilliseconds == 0 && !string.IsNullOrWhiteSpace(first.Text), "The model session reuses its loaded chat pipeline");
        Console.WriteLine("Chat response: " + first.Text);
        using TcpListener probe = new(IPAddress.Loopback, 0);
        probe.Start();
        int port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        using LocalModelApiServer server = new(model.Snapshot, model.GenerateAsync, Console.WriteLine);
        server.Start(IPAddress.Loopback, port);
        using HttpClient client = new() { BaseAddress = new(server.BaseUrl), Timeout = TimeSpan.FromSeconds(90) };
        var response = await client.PostAsJsonAsync("/v1/chat/completions", new { model = name, messages = new[] { new { role = "user", content = "What is two plus two?" } }, max_tokens = 16 });
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        string answer = json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()!;
        Check(response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(answer), "HTTP chat generates a real response using the loaded model");
        Console.WriteLine("HTTP chat response: " + answer);
        bool vision = model.Loaded!.ApiModel.SupportsImageInput;
        if (!vision)
        {
            var imageError = await client.PostAsJsonAsync("/v3/chat/completions", new { model = name, messages = new[] { new { role = "user", content = new object[]
                { new { type = "text", text = "Describe this" }, new { type = "image_url", image_url = new { url = TestImageUrl() } } } } } });
            Check(imageError.StatusCode == HttpStatusCode.BadRequest, "Text-only models reject images instead of ignoring them");
        }
        using var streamingRequest = new HttpRequestMessage(HttpMethod.Post, "/v3/chat/completions")
        {
            Content = JsonContent.Create(new { model = name, stream = true, max_tokens = 64, temperature = 0,
                messages = new[] { new { role = "user", content = vision
                    ? new object[] { new { type = "text", text = "Describe the image in one full sentence and name its main color." }, new { type = "image_url", image_url = new { url = SolidImageUrl() } } }
                    : new object[] { new { type = "text", text = "Count from one to ten in words." } } } }, stream_options = new { include_usage = true } })
        };
        using var streaming = await client.SendAsync(streamingRequest, HttpCompletionOption.ResponseHeadersRead);
        using var streamReader = new StreamReader(await streaming.Content.ReadAsStreamAsync());
        int contentChunks = 0;
        bool done = false, usageSeen = false;
        StringBuilder streamed = new();
        while (await streamReader.ReadLineAsync() is { } line)
        {
            if (line == "data: [DONE]") { done = true; break; }
            if (!line.StartsWith("data: ")) continue;
            using var item = JsonDocument.Parse(line[6..]);
            var choices = item.RootElement.GetProperty("choices");
            if (choices.GetArrayLength() == 0)
            {
                usageSeen = item.RootElement.GetProperty("usage").GetProperty("completion_tokens").GetInt32() > 0;
                continue;
            }
            if (choices[0].GetProperty("delta").TryGetProperty("content", out var delta) && delta.GetString()?.Length > 0)
            { contentChunks++; streamed.Append(delta.GetString()); }
        }
        Console.WriteLine($"Real stream (HTTP {(int)streaming.StatusCode}, {contentChunks} content chunks, done={done}, usage={usageSeen}): {streamed}");
        Check(streaming.IsSuccessStatusCode && done && usageSeen && contentChunks > 1,
            "Real model streams multiple text deltas and native token usage over /v3/chat/completions");
        if (vision) Check(streamed.ToString().Contains("red", StringComparison.OrdinalIgnoreCase), "Vision model identifies the attached red image through HTTP");
        await server.StopAsync();
    }

    private static string SolidImageUrl()
    {
        using var bitmap = new Bitmap(224, 224);
        using (var graphics = Graphics.FromImage(bitmap)) graphics.Clear(Color.Red);
        using var stream = new MemoryStream();
        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        return "data:image/png;base64," + Convert.ToBase64String(stream.ToArray());
    }

    private static async Task CheckFormAsync(string sourceXml)
    {
        string configPath = AppFiles.GetWorkbenchConfigPath();
        string? originalConfig = File.Exists(configPath) ? File.ReadAllText(configPath) : null;
        try
        {
            AppFiles.SaveModelRoot(Path.GetDirectoryName(sourceXml)!);
            Check(AppFiles.GetModelRoot() == Path.GetDirectoryName(sourceXml), "The model root still saves and loads");
            using Form1 form = new();
            _ = form.Handle;
            await form.InitializeAsync(CancellationToken.None);
            T Control<T>(string name) where T : Control => form.Controls.Find(name, true).OfType<T>().Single();
            Check(Control<ComboBox>("cboOvmsSourceModel").Items.Count == 4, "WinForms initializes all four OVMS presets");
            var tabs = Control<TabControl>("tabMain");
            Check(tabs.TabPages.Cast<TabPage>().Select(tab => tab.Name).SequenceEqual(
                new[] { "tabWorkstation", "tabDownloads", "tabOvmsPull" }), "Only the three supported tabs are present");
            Check(form.Controls.Find("lstHfRepos", true).Length == 0 && form.Controls.Find("btnUseSampleModel", true).Length == 0,
                "The obsolete catalog and sample controls are gone");
            Check(Control<TextBox>("txtLocalServerUrl").Text == "http://localhost:8000/v3", "The displayed API address uses OVMS routes on loopback");
            Check(!Control<Button>("btnCancelActivity").Enabled, "Cancel starts disabled");
            var sourcePicker = Control<ComboBox>("cboOvmsSourceModel");
            sourcePicker.SelectedIndex = 0;
            Check(Control<ComboBox>("cboOvmsPipelineType").Text.Length > 0, "Choosing an OVMS preset updates its fields");
            var target = Control<ComboBox>("cboWorkstationTarget");
            target.Items.Clear();
            target.Items.Add("CPU");
            target.SelectedIndex = 0;
            Control<TextBox>("txtInferModel").Text = sourceXml;
            typeof(Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(Control<Button>("btnLoadModel"), [EventArgs.Empty]);
            await (Task)typeof(Form1).GetField("_running", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            var session = (ModelSession)typeof(Form1).GetField("_model", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            Check(session.Loaded?.Source.Kind == ModelFileKind.OpenVinoIr, "The Load Model button validates and owns a real Core model");
            Check(session.Snapshot().Models.Count == 1, "The server exposes only the loaded model");
            Check((await session.GenerateAsync(new() { Model = "unknown", Prompt = "hello" }, CancellationToken.None)).StatusCode == 404,
                "The server does not switch to an unknown model");
            Check((await session.GenerateAsync(new() { Prompt = "hello" }, CancellationToken.None)).StatusCode == 400,
                "A Core-only model cannot be used as a chat model");
            string brokenFolder = Path.Combine(Path.GetDirectoryName(sourceXml)!, "broken");
            Directory.CreateDirectory(brokenFolder);
            string broken = Path.Combine(brokenFolder, "broken.xml");
            File.WriteAllText(broken, "<invalid>");
            Control<TextBox>("txtInferModel").Text = broken;
            typeof(Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(Control<Button>("btnLoadModel"), [EventArgs.Empty]);
            await (Task)typeof(Form1).GetField("_running", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
            Check(session.Loaded is null && Control<TextBox>("txtModelCapabilities").Text == "No model loaded.", "A failed load clears the model and its UI status");
            Check(tabs.Enabled && !Control<Button>("btnCancelActivity").Enabled, "A failed operation restores the controls");
            if (_render)
            {
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new Point(-32000, -32000);
                form.ShowInTaskbar = false;
                form.Show();
                await (Task)typeof(Form1).GetField("_running", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
                string directory = Path.Combine(AppFiles.GetProjectDirectory(), "_buildcheck", "ui");
                Directory.CreateDirectory(directory);
                foreach (TabPage tab in tabs.TabPages)
                {
                    tabs.SelectedTab = tab;
                    form.PerformLayout();
                    form.Refresh();
                    using Bitmap picture = new(form.Width, form.Height);
                    form.DrawToBitmap(picture, new Rectangle(Point.Empty, form.Size));
                    picture.Save(Path.Combine(directory, tab.Name + ".png"));
                }
            }
            await session.LoadAsync(sourceXml, "CPU", CancellationToken.None);
            form.Close();
            for (int attempt = 0; attempt < 100 && !form.IsDisposed; attempt++)
            {
                await Task.Delay(10);
            }
            Check(form.IsDisposed && session.Loaded is null, "Closing the form releases the model session");
        }
        finally
        {
            if (originalConfig is not null) File.WriteAllText(configPath, originalConfig);
            else if (File.Exists(configPath)) File.Delete(configPath);
        }
    }

    private static async Task CheckDownloadAddressesAsync()
    {
        using HttpClient client = new(new AddressResponses());
        ModelDownload download = new(client);
        var repo = (await download.FindAsync("https://huggingface.co/owner/model", CancellationToken.None)).Files;
        Check(repo.Count == 3 && repo.All(file => file.Destination.Contains(Path.Combine("model", "main"))), "A model URL resolves its files under the model's main folder");
        var folder = (await download.FindAsync("https://huggingface.co/owner/model/tree/main/subfolder", CancellationToken.None)).Files;
        Check(folder.Count == 1 && folder[0].Address.AbsolutePath.EndsWith("subfolder/weights.bin"), "A Hugging Face folder URL filters the file list");
        var file = (await download.FindAsync("https://huggingface.co/owner/model/blob/main/model.xml", CancellationToken.None)).Files;
        Check(file.Count == 1 && file[0].Address.AbsolutePath.EndsWith("/resolve/main/model.xml"), "A Hugging Face file page resolves to its download URL");
        var direct = (await download.FindAsync("https://fixture/model.onnx", CancellationToken.None)).Files;
        Check(direct.Count == 1 && direct[0].Destination.EndsWith("model.onnx"), "Direct file URLs retain their download path");
    }

    private sealed class AddressResponses : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""
                {"siblings":[{"rfilename":"model.xml"},{"rfilename":"model.bin"},{"rfilename":"subfolder/weights.bin"}]}
                """) });
    }

    private sealed class DownloadResponses : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpContent content = request.RequestUri!.AbsolutePath == "/slow"
                ? new StreamContent(new SlowStream())
                : new ByteArrayContent(Encoding.UTF8.GetBytes(request.RequestUri.AbsolutePath == "/valid" ? "GGUFtest" : "bad"));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        }
    }

    private sealed class SlowStream : MemoryStream
    {
        public SlowStream() : base(new byte[200000]) { }
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await Task.Delay(300, cancellationToken);
            return await base.ReadAsync(buffer, cancellationToken);
        }
    }
}
