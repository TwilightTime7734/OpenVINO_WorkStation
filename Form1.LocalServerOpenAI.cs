namespace WorkStationC_;

public partial class Form1
{
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ConfigureOpenAiCompatibleLocalServerUi();
    }

    private void ConfigureOpenAiCompatibleLocalServerUi()
    {
        // Rebrand the server surface without disturbing the existing model/runtime code.
        tabServerTests.Text = "Local Server";
        lblOpenVinoRuntimeHeading.Text = "OpenAI-compatible API endpoints";
        lblServerNoInputHeading.Text = "OpenAI-compatible GET checks";
        lblServerUserInputHeading.Text = "OpenAI-compatible generation requests";
        lblTestEndpoint.Text = "Health endpoint:";
        btnTestEndpoint.Text = "Test Health";
        lblModelsEndpoint.Text = "Models endpoint:";
        lblChatEndpoint.Text = "Chat Completions:";
        lblTextEndpoint.Text = "Completions:";
        lblImageEndpoint.Text = "Image Generations:";
        lblLocalServerUrl.Text = "Local server URL:";
        lblLocalServerStatus.Text = "Local server status:";
        btnStartLocalServer.Text = "Start Local Server";
        btnStopLocalServer.Text = "Stop Local Server";
        lblTestApiKey.Text = "API key (optional):";
        lblSubtitle.Text = "Tabbed runner for device checks, model tests, conversion, downloads, and an OpenAI-compatible local server.";

        // Replace the generic/Ollama-oriented handlers with OpenAI-compatible local-server handlers.
        btnStartLocalServer.Click -= btnStartLocalServer_Click;
        btnStartLocalServer.Click += btnStartOpenAiLocalServer_Click;
        btnStopLocalServer.Click -= btnStopLocalServer_Click;
        btnStopLocalServer.Click += btnStopOpenAiLocalServer_Click;
        btnTestEndpoint.Click -= btnTestEndpoint_Click;
        btnTestEndpoint.Click += btnTestHealthEndpoint_Click;
        btnShowServerSettings.Click -= btnShowServerSettings_Click;
        btnShowServerSettings.Click += btnShowOpenAiServerSettings_Click;
        btnHelp.Click -= btnHelp_Click;
        btnHelp.Click += btnOpenAiAwareHelp_Click;

        ApplyOpenAiCompatibleEndpointDefaults();
    }

    private void ApplyOpenAiCompatibleEndpointDefaults()
    {
        string baseUrl = $"http://localhost:{LocalModelApiServerPort}";
        txtLocalServerUrl.Text = baseUrl;
        txtTestEndpoint.Text = $"{baseUrl}/health";
        txtModelsEndpoint.Text = $"{baseUrl}/v1/models";
        txtChatEndpoint.Text = $"{baseUrl}/v1/chat/completions";
        txtTextEndpoint.Text = $"{baseUrl}/v1/completions";
        txtImageEndpoint.Text = $"{baseUrl}/v1/images/generations";
        txtServerEndpointList.Text = string.Join(
            Environment.NewLine,
            new[]
            {
                "Protocol: OpenAI-compatible",
                $"Health: {baseUrl}/health",
                $"Models: {baseUrl}/v1/models",
                $"Chat Completions: {baseUrl}/v1/chat/completions",
                $"Completions: {baseUrl}/v1/completions",
                $"Image Generations (not implemented): {baseUrl}/v1/images/generations",
            });
    }

    private void btnStartOpenAiLocalServer_Click(object? sender, EventArgs e)
    {
        if (_localApiServer.IsRunning)
        {
            AppendOutput($"OpenAI-compatible local server is already running at {_localApiServer.BaseUrl}.");
            UpdateLocalApiServerControls();
            ApplyOpenAiCompatibleEndpointDefaults();
            return;
        }

        try
        {
            _localApiServer.Start(
                System.Net.IPAddress.Loopback,
                LocalModelApiServerPort,
                "localhost");
            UpdateLocalApiServerControls();
            ApplyOpenAiCompatibleEndpointDefaults();

            string baseUrl = _localApiServer.BaseUrl;
            AppendOutput("OpenAI-compatible local server started.");
            AppendOutput($"  Listening: localhost (127.0.0.1) on port {LocalModelApiServerPort}");
            AppendOutput($"  Health: {baseUrl}/health");
            AppendOutput($"  Models: {baseUrl}/v1/models");
            AppendOutput($"  Chat Completions: {baseUrl}/v1/chat/completions");
            AppendOutput($"  Completions: {baseUrl}/v1/completions");
            AppendOutput($"  Image Generations (not implemented): {baseUrl}/v1/images/generations");

            if (_loadedRuntimeModelKind == ModelFileKind.Unknown || string.IsNullOrWhiteSpace(_loadedRuntimeModelPath))
            {
                AppendOutput("  No Workstation model is loaded yet. The Models endpoint can still list known model folders.");
            }
        }
        catch (System.Net.Sockets.SocketException ex)
        {
            AppendOutput($"OpenAI-compatible local server could not start on localhost port {LocalModelApiServerPort}: {ShortError(ex)}");
            if (ex.SocketErrorCode == System.Net.Sockets.SocketError.AddressAlreadyInUse)
            {
                AppendOutput("  Another program is already using that port.");
            }

            UpdateLocalApiServerControls();
            ApplyOpenAiCompatibleEndpointDefaults();
        }
        catch (Exception ex)
        {
            AppendOutput($"OpenAI-compatible local server could not start: {ShortError(ex)}");
            UpdateLocalApiServerControls();
            ApplyOpenAiCompatibleEndpointDefaults();
        }
    }

    private async void btnStopOpenAiLocalServer_Click(object? sender, EventArgs e)
    {
        if (!_localApiServer.IsRunning)
        {
            AppendOutput("OpenAI-compatible local server is already stopped.");
            UpdateLocalApiServerControls();
            ApplyOpenAiCompatibleEndpointDefaults();
            return;
        }

        await StopLocalApiServerAsync(reportStatus: false);
        ApplyOpenAiCompatibleEndpointDefaults();
        AppendOutput("OpenAI-compatible local server stopped.");
    }

    private async void btnTestHealthEndpoint_Click(object? sender, EventArgs e)
    {
        string endpoint = txtTestEndpoint.Text.Trim();
        if (!TryCreateAbsoluteUri(endpoint, out Uri? uri) || uri is null)
        {
            AppendOutput($"Local server health test: invalid URL `{endpoint}`.");
            return;
        }

        if (IsThisAppsLocalServerUri(uri) && !_localApiServer.IsRunning)
        {
            AppendOutput("Local server health test: local server is stopped. Open Workstation and click Start Local Server first.");
            return;
        }

        btnTestEndpoint.Enabled = false;
        try
        {
            AppendOutput($"Local server health test: GET {uri}");
            using System.Net.Http.HttpRequestMessage request = new(System.Net.Http.HttpMethod.Get, uri);
            AddOptionalBearerToken(request);

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using System.Net.Http.HttpResponseMessage response = await EndpointTestClient.SendAsync(request);
            string body = await response.Content.ReadAsStringAsync();
            stopwatch.Stop();

            AppendOutput($"  Status: {(int)response.StatusCode} {response.ReasonPhrase}");
            AppendOutput($"  Time: {stopwatch.ElapsedMilliseconds:N0} ms");
            AppendOutput($"  Content-Type: {response.Content.Headers.ContentType?.ToString() ?? "(none)"}");
            AppendOutput($"  Body: {TrimForOutput(body, 700)}");
        }
        catch (Exception ex)
        {
            AppendOutput($"Local server health test failed: {ShortError(ex)}");
        }
        finally
        {
            btnTestEndpoint.Enabled = true;
        }
    }

    private void btnShowOpenAiServerSettings_Click(object? sender, EventArgs e)
    {
        string healthUrl = txtTestEndpoint.Text.Trim();
        string modelsUrl = txtModelsEndpoint.Text.Trim();
        string chatUrl = txtChatEndpoint.Text.Trim();
        string textUrl = txtTextEndpoint.Text.Trim();
        string imageUrl = txtImageEndpoint.Text.Trim();
        string serverModel = cboServerModel.Text.Trim();
        string prompt = txtChatPrompt.Text.Trim();
        string apiKey = txtTestApiKey.Text;

        AppendOutput("OpenAI-compatible local server settings");
        AppendOutput($"  Local server URL: {FormatSettingValue(txtLocalServerUrl.Text)}");
        AppendOutput($"  Local server status: {FormatSettingValue(txtLocalServerStatus.Text)}");
        AppendOutput($"  Health URL: {FormatSettingValue(healthUrl)}");
        AppendOutput($"    Check: {DescribeUrlCheck(healthUrl, expectedPathHint: "/health")}");
        AppendOutput($"  Models URL: {FormatSettingValue(modelsUrl)}");
        AppendOutput($"    Check: {DescribeUrlCheck(modelsUrl, expectedPathHint: "/v1/models")}");
        AppendOutput($"  Chat Completions URL: {FormatSettingValue(chatUrl)}");
        AppendOutput($"    Check: {DescribeUrlCheck(chatUrl, expectedPathHint: "/chat/completions")}");
        AppendOutput($"  Completions URL: {FormatSettingValue(textUrl)}");
        AppendOutput($"    Check: {DescribeUrlCheck(textUrl, expectedPathHint: "/completions")}");
        AppendOutput($"  Image Generations URL: {FormatSettingValue(imageUrl)}");
        AppendOutput($"    Check: {DescribeUrlCheck(imageUrl, expectedPathHint: "/images")}");
        AppendOutput($"  Server model: {FormatSettingValue(serverModel)}");
        AppendOutput($"  API key: {(string.IsNullOrWhiteSpace(apiKey) ? "not set" : $"set ({apiKey.Length:N0} characters, hidden)")}");
        AppendOutput($"  Prompt: {FormatSettingValue(TrimForOutput(prompt, 500))}");
    }

    private void btnOpenAiAwareHelp_Click(object? sender, EventArgs e)
    {
        if (tabMain.SelectedTab != tabServerTests)
        {
            ShowActiveTabHelp();
            return;
        }

        AppendOutput(string.Empty);
        AppendOutput("Help - Local Server");
        AppendOutput("------------------------------------------------------------");
        AppendOutput("Local Server exposes the loaded Workstation model through an OpenAI-compatible HTTP API.");
        AppendOutput("The built-in server listens only on localhost (127.0.0.1) by default.");
        AppendOutput("Health uses /health and model discovery uses /v1/models.");
        AppendOutput("Chat uses /v1/chat/completions and classic text completion uses /v1/completions.");
        AppendOutput("The Image Generations route is reserved but is not implemented yet.");
        AppendOutput("Test Models fills the model dropdown from an OpenAI-style /v1/models response.");
        AppendOutput("The built-in local server does not require an API key; the field is available for testing other compatible servers.");
        AppendOutput("All results and errors appear in the shared Output panel.");
        AppendOutput(string.Empty);
    }
}
