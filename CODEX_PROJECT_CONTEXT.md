# WorkStationC# - Codex Project Context

This file is the working source-of-truth for future Codex sessions. Keep it clean, current, and practical. Remove stale notes instead of adding contradictory layers.

## Non-negotiable WinForms Designer and event rules

These rules must never be removed, shortened away, or overridden during context updates:

- `Form1.Designer.cs` owns UI design: control creation, layout, sizing, labels, colors, fonts, anchors, docking, tab order, and named event hookups.
- `Form1.cs` owns the actual `Form1 : Form` code-behind: constructor/bootstrap, named event handlers, shared behavior, tab behavior, and runtime plumbing.
- Do not create new `MainForm.*.cs` partial `Form1` files. They were removed because Visual Studio Community repeatedly classified partial `Form1` behavior files as designer Forms and wrote bad `WorkStationC#.csproj.user` metadata.
- If a new feature grows too large for `Form1.cs`, move it into a normal non-Form helper/controller/service class that does not inherit from `Form` and is not `partial class Form1`. Keep Designer event hookups named, with thin named handlers in `Form1.cs` delegating to that helper.
- `WorkStationC#.csproj.user` is per-user Visual Studio state and must not be treated as project source. It is ignored by `.gitignore`; if VS recreates it with stale `MainForm.*.cs` entries, close VS and delete the `.csproj.user` file.
- Every user-interface control event must use a named event-handler method implemented in `Form1.cs`.
- Do not use lambdas or anonymous delegates for control events such as `Click`, `TextChanged`, `SelectedIndexChanged`, drag/drop, keyboard, tab, drawing, or similar events.
- Designer controls must be wired by name in `Form1.Designer.cs`, for example `btnLoad.Click += btnLoad_Click;`.
- Double-clicking a control or opening its event in Visual Studio Properties must lead to the real named handler in `Form1.cs`; it must not create an empty duplicate handler.
- Runtime code must not rename, reposition, resize, hide, or restyle Designer-owned controls unless the behavior truly has to be dynamic. If a runtime visual change is unavoidable, add a short comment explaining why.
- Before completing any UI change, search for newly introduced anonymous control-event handlers and reject the change if any were added.
- Designer event-handler compliance command: `rg -n "\\.(Click|TextChanged|SelectedIndexChanged|DragDrop|DragEnter|DragLeave|KeyDown|Leave|DrawItem|DrawColumnHeader|DrawSubItem|Selecting)\\s*\\+=\\s*(async\\s*)?\\(" Form1.Designer.cs Form1.cs`. The required result is zero anonymous control-event matches.

## Non-negotiable Windows line-ending rule

- All repository text files must use Windows `CRLF` line endings.
- New files must be created with `CRLF`, and edited files must retain or be normalized to `CRLF` before the work is completed.
- Do not leave `LF`-only or mixed line endings in any file touched by a change. Verify the line endings of every text file changed.
- This project is maintained with Visual Studio Community on Windows; non-Windows line endings cause editor, Designer, build, and source-control trouble.

## Non-negotiable root-cause and source-of-truth rule

This rule must never be removed, shortened away, or overridden during context updates:

- Never hide, bypass, compensate for, or patch around an error merely to make the symptom disappear.
- Find and correct the actual cause at its authoritative source of truth.
- Do not add startup fixups, duplicate state, fallback UI mutations, catch-and-ignore behavior, or special-case workarounds when the underlying Designer definition, model state, path resolver, configuration, or core logic can be corrected directly.
- If the source of truth cannot be identified or safely changed, stop and report that clearly instead of applying a workaround without the user's explicit approval.
- After correcting the source, remove obsolete patches and verify the original failing path directly.

## Non-negotiable plain-language UI rule

- User-facing labels, buttons, status messages, and Help must use ordinary language that a non-developer can understand.
- Avoid developer-only terms such as `smoke test`, `tensor`, `pipeline`, `runtime entry`, and `inference request` in normal controls or instructions.
- Prefer familiar actions such as `Send Message`, `Run Image Test`, `Run Model Test`, `Load Model`, `Working`, and `Cancel`.
- Chat interaction must be obvious: open `Tests`, type into `Test Text`, then click `Send Message`.
- The Tests text action button must remain `Send Message` as defined in `Form1.Designer.cs`; runtime code must not rename it to `Run Model Test`.
- Before completing a UI change, review every new visible string from the perspective of a first-time user.

## Project purpose

This project is a C# WinForms recreation of a useful Python OpenVINO workstation workflow. It is not meant to be a line-by-line Python translation.

The finished program should feel self-contained to the user. It should not require Visual Studio, NuGet restore, OpenVINO installed globally, or a separate helper program on the target machine. Runtime dependencies should be bundled deliberately.

## Model storage and compiled blob rules

- The configured main model folder must contain model folders only. Do not create app-owned root helper folders such as `downloads`, `direct`, `local`, `converted`, `compiled-cache`, or `results` inside the main model folder.
- Downloads, including direct web-file downloads, must use `MainModelFolder\<model-name>\main\...`.
- App bookkeeping such as `workbench.cfg` and local-model index files belongs beside the app/project, not inside the main model folder.
- Benchmark, chart, and image test results are not saved to disk by default. Do not recreate a `results` folder unless the user explicitly asks for saved reports.
- The Convert tab is the only app-owned place that should create/export new OpenVINO compiled `.blob` files.
- Workstation and Tests may import and run existing compiled `.blob` files, but they must not auto-create compiled-cache blobs.
- Keep compiled blobs with the specific model they belong to; do not create new root-level blob cache folders.
- New Convert exports must create `MainModelFolder\<model-name>\<target>\model.blob` beside the model's `main` folder, such as `MainModelFolder\<model-name>\main\...` plus `MainModelFolder\<model-name>\CPU\model.blob`, `...\GPU\model.blob`, or `...\MULTI\model.blob`. Do not create `MainModelFolder\<optimized-model-name>\...`, `MainModelFolder\converted\...`, or any other separate converted/cache folder for new exports.
- Convert is now GGUF-focused. The visible Convert tab should have only a GGUF model dropdown populated from the Workstation main model folder, a Refresh Models button, `.blob` and `.xml/.bin` output checkboxes, and one Convert button.
- The Convert tab must not show the old model-folder label, file list box, model-file text box, output text box, Optimize for Target button, Validate Target button, Use Model button, or Convert Help button unless the user explicitly asks to bring them back.
- Supported `.gguf` chat models are saved to `.xml/.bin` through OpenVINO GenAI by creating an `LLMPipeline` with `enable_save_ov_model=true` and generating a tiny response to trigger OpenVINO IR serialization.
- GGUF-to-IR conversion is not guaranteed for every GGUF. The bundled OpenVINO GenAI GGUF reader has architecture, tokenizer, and quantization limits; supported GGUF files may also be run directly through GenAI without saving `.xml/.bin`.
- Do not delete or advise deleting the original `.gguf` until the generated `.xml/.bin` folder loads and generates correctly. Native OpenVINO IR text-model downloads must keep their tokenizer/detokenizer files beside the model when provided.
- When `.blob` is checked, Convert must use the Workstation tab's selected `Models target:` value. The app creates/saves the GGUF IR first, then compiles that generated `.xml/.bin` with OpenVINO Core for the selected target and exports `model.blob`.
- When both checkboxes are selected, one Convert action should create both outputs.
- `.blob` output remains target/runtime-specific and must be written beside the model's `main` folder as `MainModelFolder\<model-name>\<target>\model.blob`.
- `.onnx`, `.pb`, `.caffemodel`, `.tflite`, `.pdmodel`, `.safetensors`, PyTorch checkpoints, support files, TensorRT engines, and training/data formats are not part of the simplified Convert tab.
- The old Load Model tab local-library UI was removed. That tab is now the Optimum Intel tab.
- Optimum Intel export must stay separate from the GGUF Convert tab.
- The portable Optimum Intel helper is bundled in `Runtime\optimum-exporter\`. Keep the whole folder intact: `optimum-exporter.exe` requires the sibling `_internal` folder created by PyInstaller. Do not copy or launch the `.exe` by itself.
- WorkStation should prefer the bundled helper at `Runtime\optimum-exporter\optimum-exporter.exe` and must not require the end user to install Python, pip, Optimum Intel, Transformers, PyTorch, or OpenVINO globally.
- The bundled helper exposes `export`, `validate`, and `self-check` commands. WorkStation should run it as a child process, pass explicit model/output/task/weight-format/trust options, capture stdout/stderr into Output, and report a plain missing-helper message if the runtime folder is absent or incomplete.
- The installed `optimum-cli` command, or the Python module form `python -m optimum.commands.optimum_cli` / `py -m optimum.commands.optimum_cli`, is now a developer fallback only. Do not make normal WorkStation runtime behavior depend on these installed commands unless the user explicitly asks.
- `WORKSTATION_OPTIMUM_CLI` may point to a specific helper command path for developer testing, but the bundled `Runtime\optimum-exporter\optimum-exporter.exe` remains the default portability path.
- Optimum Intel sources may be a downloaded Hugging Face source folder under the main model root, a typed Hugging Face model ID, or a typed local model folder. Local source folders normally need an explicit task such as `text-generation-with-past`.
- Optimum Intel output must end up in `MainModelFolder\<model-name>\main\...` so Workstation can load the exported OpenVINO files normally. Do not create root-level `converted`, `compiled-cache`, or other helper folders for Optimum output.
- The bundled helper's current role is OpenVINO IR export and validation: `.xml/.bin` plus tokenizer/config support files. If Optimum Intel `.blob` creation is added later, it must use the Workstation tab's selected `Models target:` value and write `MainModelFolder\<model-name>\<target>\model.blob`, matching the existing compiled blob rule.
- Optimum Intel can execute model repository code only when the user checks the visible Allow model code option. Do not enable trust/remote-code behavior silently.

## Model browsing and download source rules

- Downloads is intentionally KISS: a user-editable Web Address text box plus a Download button are the primary model download workflow.
- The user can paste a Hugging Face model page, tree page, or file page, such as `https://huggingface.co/Qwen/Qwen3-Coder-Next-FP8/tree/main`, and click Download. The app should download the files under that repo/tree into the configured main model folder.
- The Downloads tab has a Find button that checks the pasted Web Address before downloading. For Hugging Face tree/model/file addresses it should confirm the repo, revision, and matching file count; for direct file links it should confirm that the URL responds before saving anything.
- The Downloads tab has twelve programmable favorite-website buttons. Each button opens its saved website in the default browser and also fills the Web Address box.
- Favorite websites are saved with a single Save Site action: the user enters Web Address and Button Label, chooses a Save To slot, and clicks Save Site.
- The main model folder and the twelve favorite website labels/URLs are persisted in `workbench.cfg` beside the project/app config. `model-root.txt` is legacy and must not be recreated as the active config file.
- Hugging Face downloads go directly under `MainModelFolder\<model-name>\main\...`, so Workstation can select the model folder and inspect its `main` contents.
- Keep any remaining in-app model listing/search code as hidden helper implementation only unless the user explicitly asks to bring it back.

## New goal: local model API server

The next major direction is to let this program act as a simple local model server after a model is loaded.

The intended flow:

1. User selects a main model folder, model folder, and runtime target in Workstation.
2. User loads or compiles the model in this app.
3. The app keeps the loaded model alive while the program is open.
4. User starts a local API server from this app.
5. Other local programs can call this app over HTTP to use the loaded model.
6. Closing the app or stopping the server releases the loaded model/server resources.

Initial server scope should be deliberately small:

- Initial implementation is present: private-network TCP HTTP listener bound to all adapters (`0.0.0.0`) on port `11434`.
- Initial implementation is present: Start Server / Stop Server UI owned by `Form1.Designer.cs`, with named handlers in `Form1.cs`.
- Initial implementation is present: `GET /api/tags` lists loadable direct child folders of the configured main model root, plus the currently loaded Workstation model if it is not already in that root-folder list.
- Initial implementation is present: `GET /v1/models` lists the same models in OpenAI-style model-list format.
- Server model-list endpoints must not recursively list models from app-managed folders such as `downloads`, `converted`, `local`, `compiled-cache`, or `results`; only the currently loaded Workstation model may appear from outside the root-folder list.
- Initial implementation is present: `GET /health` and `GET /api/health` report server status.
- Initial implementation is present: `POST /api/search` runs web search through configured external providers and returns normalized title, URL, snippet, source, and rank data.
- Web search providers are implemented behind `WebSearchService` with Serper and Brave Search support. API keys must come from environment variables, not source code: `WORKSTATION_SERPER_API_KEY` or `SERPER_API_KEY`, and `WORKSTATION_BRAVE_SEARCH_API_KEY` or `BRAVE_SEARCH_API_KEY`.
- The `/api/search` endpoint is served by this app on the configured private-network address, but selected providers are external services; search requests leave the computer for Serper or Brave when that provider is used.
- `POST /v1/chat/completions` and `POST /v1/completions` are wired to the loaded Workstation text model for OpenVINO GenAI and GGUF model types through the shared long-operation/native-safety gate.
- `/v1/chat/completions` is intended for VS Code/Codex-style OpenAI-compatible clients. It accepts extra chat fields such as `tools`, `tool_choice`, `parallel_tool_calls`, `response_format`, tool-result messages, and `stream`.
- The `model` field is enforced. If it is blank, the loaded Workstation model is used. If it names the currently loaded model, the request runs. If it names a known text-capable model that is not loaded, the server selects it in Workstation, loads it through the shared long-operation/native-safety gate, marks it loaded after successful generation, and runs the request. If it names an unknown model, the server returns `model_not_found`.
- The local server does not execute tools. When tools are supplied, the request is shaped so the loaded model can ask for a tool call; the external client remains responsible for running tools and sending `role: "tool"` results back.
- Streaming is compatibility streaming: the server generates the response through the existing loaded model path, then returns OpenAI-style SSE chunks and `[DONE]`.
- `POST /v1/images/generations` currently returns clear not-implemented JSON. Add real image routes only after model-specific generation/editing support is implemented.
- Reuse the loaded Workstation model state; do not create a separate server-only model picker unless the user explicitly asks.
- Use plain Output messages that explain whether the server is stopped, running, missing a loaded model, or receiving unsupported requests.
- Current user-approved network mode is private Wi-Fi/personal network access. The server listens on all adapters and advertises `192.168.0.11:11434` only if that address is assigned to this computer; otherwise it advertises the first private IPv4 address Windows reports. Do not change it back to loopback unless the user asks.
- Do not expose the server to the internet unless the user explicitly asks and the risks are discussed.

The existing URL test controls still act as an HTTP client. Current UI ownership: Workstation owns local server Start/Stop, address, and status. Server Tests owns the endpoint list, endpoint URL boxes, endpoint test buttons for tags/models/completions/text/image, and Show URLs.

## What not to do

- Do not publish a final release unless the user asks.
- Do not convert the old Python project in one pass.
- Do not replace WinForms with WPF, MAUI, Avalonia, or another UI framework.
- Do not downgrade from .NET 10.
- Do not make app/runtime code depend on `/Extras`.
- Do not assume a model supports image input; inspect model capability first.
- Do not use hidden runtime patches to fix Designer mistakes. Fix `Form1.Designer.cs`.

## Communication style for this user

The user is technically capable and wants practical results.

- Be direct.
- Use plain language in the app and in explanations.
- Show exact file names and build errors.
- Avoid vague statements.
- When something cannot be verified, say exactly what remains unverified.
- Prefer incremental implementation over a large speculative rewrite.
