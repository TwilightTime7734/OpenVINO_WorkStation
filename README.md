# OpenVINO WorkStation

OpenVINO WorkStation is an all-in-one Windows desktop workbench for finding, downloading, pulling, serving, and chatting with OpenVINO models. It brings the model workflow and chat tools together in one interface, from getting a model to using it through OpenVINO Model Server (OVMS).

## What you can do

- **Downloads:** Find Hugging Face model files or a direct file URL, download them, and get a recommendation for when to use OVMS Pull.
- **OVMS Pull:** Configure and preview pulls for Hugging Face exports or GGUF files, check the required tools and versions, and start the pull. Downloads can prefill the pull settings.
- **Workstation:** Select a model and device, start or stop OVMS, or connect to a server that is already running.
- **Chat:** Stream replies from the server, attach images for compatible vision models, and use tools for web page reading, web search, files, and commands when enabled.

Chat tools are optional: enable **Let the model use tools**. Search requires a Brave or Serper key. File and command tools require a selected working folder; the model can list, read, and write files in that folder and its subfolders without asking. Access outside that folder asks for permission. Commands ask for approval unless **Run commands without asking** is enabled. Use these capabilities only with models and working folders you trust.

## Requirements and setup

- Windows with the .NET 10 SDK for building; the app uses Windows Desktop and ASP.NET Core 10 runtimes.
- An OVMS installation for starting a server and using OVMS Pull. Set the OVMS executable and its Python runtime paths in the OVMS Pull tab. Hugging Face export also requires the compatible Python export tools checked by the app. Models and their hardware requirements vary.
- Git LFS to obtain the repository's native runtime files. The build also supplies OpenVINO runtime packages through NuGet.

The included `ovms-settings.json` contains example defaults and machine-specific paths. Review and adjust the paths and settings for your machine before using OVMS. Downloaded models and external tools are not included in the repository.

Build from the project directory:

```powershell
dotnet build 'WorkStationC#.csproj'
```

Run the built application, configure your OVMS paths, choose a model folder and target in **Workstation**, then select **Load Model**. Alternatively, connect to an existing server. Use **Downloads** and **OVMS Pull** to obtain or prepare models, and **Chat** to interact with the served model.

## Optional web search keys

Create a private `SearchApiKeys.txt` in the project directory for local development, or next to the executable for a published app. Provide either or both keys:

```text
Brave Search API-Key: YOUR_BRAVE_API_KEY
Serper Search API-Key: YOUR_SERPER_API_KEY
```

Get your own keys from [Brave Search API](https://brave.com/search/api/) and/or [Serper](https://serper.dev/). Omit or leave blank any provider you do not use. Brave is tried first, with Serper as a fallback if configured. Without a key, web search is unavailable, but page reading remains available.

The app checks next to the executable first, then the project directory. A local build copies the project file next to the executable; update that copy too or rebuild after changing a key. The key file is Git-ignored and excluded from publish output. Never commit, force-add, or distribute it; check older publish folders for leftover copies before sharing them.

For implementation details, see [CODE_MAP.md](CODE_MAP.md).
