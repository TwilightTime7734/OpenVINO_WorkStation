# Code map

`Form1.cs` keeps the form small: startup, named event handlers, output, progress, and shutdown. Each tab (Workstation, Chat, Downloads, OVMS Pull) has its own class.

| File | Responsibility |
| --- | --- |
| `Form1.cs` | Startup, events, progress, output, and shutdown. |
| `Form1.Designer.cs` | Controls, layout, and named event hookups. |
| `Workstation/WorkstationTab.cs` | Select folders and targets, discover devices, load a model into OVMS. |
| `Workstation/OvmsServer.cs` | Start, watch, and stop the `ovms.exe` process that serves the selected model. |
| `Chat/ChatTab.cs` | Chat tab: model list, transcript, images, send. |
| `Chat/OvmsChat.cs` | Streamed `/v3/chat/completions` calls and conversation history. |
| `Downloads/DownloadTab.cs` | Find and download actions with progress. |
| `OVMS_Pull/OvmsTab.cs` | Presets, settings fields, command preview, and starting a pull. |
| `Runtime/ModelSession.cs` | Model folder resolution (`Resolve`). The in-app loader is no longer used by the UI. |
| `Runtime/ChatModel.cs` | Cache and use one LLM or VLM pipeline. |
| `Runtime/ModelImages.cs` | Load chat image attachments and own their RGB image tensors. |
| `Runtime/OpenVinoRuntime.cs` | Configure the bundled native runtime. |
| `Workstation/WorkstationInfo.cs` | OpenVINO device choices and multi-GPU settings. |
| `Downloads/ModelDownload.cs` | Parse model URLs and obtain Hugging Face file lists. |
| `Downloads/FileDownload.cs` | Transfer and validate files; remove interrupted partial files. |
| `OVMS_Pull/OvmsPull.cs` | OVMS settings, command scripts, and dependency checks. |
| `Models/ModelTypes.cs` | Model enums. |
| `Models/ModelFiles.cs` | Find model files and classify the loading path. |
| `Common/AppFiles.cs` | Configuration and model/download paths. |
| `LocalServer/ModelMetadata.cs` | Describe the loaded model for the API. |
| `LocalModelApiServer.cs` | Old in-app HTTP server. Not used by the UI; OVMS serves the model now. |

## Build and check

Use the .NET 10 SDK on Windows. The app requires Windows Desktop and ASP.NET Core 10 runtimes. The build supplies OpenVINO native packages.

```powershell
dotnet build 'WorkStationC#.csproj'
dotnet run --project 'Checks/WorkstationChecks.csproj'
```

The checks cover downloads and URLs, configuration, model classification, OVMS arguments, HTTP behavior and shutdown, direct OpenVINO model loading, button wiring, failure recovery, and form shutdown. Temporary fixtures are removed afterward.

## Optional web search keys

Chat can use Brave Search or Serper for web search. Create `SearchApiKeys.txt` in the project directory for local development, or next to the executable for a published app, with either or both keys:

```text
Brave Search API-Key: YOUR_BRAVE_API_KEY
Serper Search API-Key: YOUR_SERPER_API_KEY
```

Obtain your own keys from [Brave Search API](https://brave.com/search/api/) and/or [Serper](https://serper.dev/). Omit a line or leave its value blank if you do not use that provider. Brave is tried first; Serper is used if Brave fails and a Serper key is configured. Without either key, web search is unavailable, but reading web pages remains available.

The app checks next to the executable first, then the project directory. A local build copies the project file next to the executable; update that copy too or rebuild after changing a key. The file is Git-ignored and excluded from publish output. Never commit it, force-add it, or distribute it with a release; check existing publish directories for leftover copies before sharing them.

## Model checks

To also check a real text model and HTTP chat, replace the example model path and device with your own:

```powershell
dotnet run --project 'Checks/WorkstationChecks.csproj' -- '<model-directory>' 'GPU.0'
```

## Behavior

- Select a model and target, then Load Model. Selection changes take effect when Load Model is clicked.
- Native OpenVINO models are read directly by OpenVINO Core. No converted model files are created or imported.
- The local server uses that loaded model. `/v3/models` and `/v1/models` list it; requesting another model returns 404. Automatic model discovery and switching are removed.
- The server binds to `http://localhost:8000` and displays `http://localhost:8000/v3` as its API address. OVMS `/v3` routes expose model listing/retrieval, chat completions, and text completions; `/v1` remains an alias. `/v2/health/live` and `/v2/health/ready` report server health; `/health` describes the server and loaded model. See `LocalServer/README.md` for Open WebUI setup and supported features.
- Streaming forwards native callbacks immediately as flushed SSE events, with cancellation, finish reasons, and optional token usage. Text-only chat preserves message roles; text completions use a plain prompt.
- Vision chat accepts base64 image data URLs and HTTP(S) image URLs, converts images to RGB, and supplies them to the loaded VLM. Image markers preserve attachment order in the VLM text transcript. Image generation/editing endpoints still return HTTP 501.
- Downloads use a URL, Find, and Download. The catalog browser, file inspector, bookmarks, and separate download Cancel button are removed. The shared Cancel button stops the active operation.
- `workbench.cfg` stores the model root. Existing unrelated config lines are preserved. `ovms-settings.json` holds OVMS defaults, presets, paths, and expected package versions.
- Downloads use `<model-root>/<model>/main/...`.
- Closing the app cancels active work, stops the server, waits for native work, and releases the model.
