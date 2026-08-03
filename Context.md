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

This project is a C# WinForms recreation of a useful Python/OpenVINO workstation workflow. It is not meant to be a line-by-line Python translation.

The normal Workstation, Tests, Convert-to-blob, Downloads, and local-server workflows should feel self-contained to the user. Runtime dependencies should be bundled deliberately when that is practical.

OVMS `--pull` export is currently different: it depends on the user's installed `C:\Example\OpenVINO\ovms.exe`, the OVMS bundled Python DLL folder, and a known-good global Python export stack. Do not pretend this path is fully standalone. The app should check that stack, explain mismatches plainly, and recommend the exact repair commands before starting a non-GGUF pull.

## OVMS Pull and known-good export stack

This section replaces the removed old `handoff\HANDOFF.md` package as the project source of truth.

### Hardware and architecture assumptions

- Windows host.
- Two Intel Arc Pro B70 GPUs, 32 GB VRAM each.
- The display is forced to CPU/integrated graphics in BIOS so the B70 VRAM is available.
- The old round-robin model-loading idea was removed. Do not reintroduce it.
- The required architecture is OpenVINO/OVMS, using OpenVINO's own model loading and split-device behavior.

### Important OVMS paths

```text
OVMS installation:      C:\Example\OpenVINO
OVMS executable:        C:\Example\OpenVINO\ovms.exe
Bundled Python runtime: C:\Example\OpenVINO\python\python.exe
Bundled Python DLL:     C:\Example\OpenVINO\python\python312.dll
Model repository:       C:\Example\Models
Global Python:          C:\Users\ExampleUser\AppData\Local\Python\pythoncore-3.14-64\python.exe
Global optimum-cli:     C:\Users\ExampleUser\AppData\Local\Python\pythoncore-3.14-64\Scripts\optimum-cli.exe
```

Confirmed OVMS build:

```text
OpenVINO Model Server 2026.2.1.1122f03bf
OpenVINO backend 2026.2.1-21919-ede283a88e3-releases/2026/2
OpenVINO GenAI backend 2026.2.1.0-3123-7dea0459b2a
Bazel build flags: --config=win_mp_on_py_on
```

### Two Python roles

Do not confuse these:

- `ovms.exe` needs its bundled Python 3.12 DLL at startup. Before starting OVMS in a fresh CMD process, prepend `C:\Example\OpenVINO\python` to `PATH`.
- During `ovms.exe --pull`, OVMS launches the `optimum-cli.exe` found on `PATH`. The successful Qwen conversions used the global Python 3.14 installation and its global `optimum-cli.exe`.
- This is intentional: OVMS loads `python312.dll`, while the export subprocess runs through global Python 3.14.

Known-good global export stack:

```text
Python             3.14.6
transformers       5.2.0
optimum-intel      2.1.0.dev0
optimum            2.2.0
openvino           2026.2.1
huggingface-hub    1.21.0, but query live before treating Hub as exact
accelerate         1.14.0 installed during troubleshooting
```

The critical fix was installing Optimum Intel from main:

```cmd
"C:\Users\ExampleUser\AppData\Local\Python\pythoncore-3.14-64\python.exe" -m pip install --upgrade --force-reinstall --no-deps "https://github.com/huggingface/optimum-intel/archive/refs/heads/main.zip"
```

That produced `optimum-intel 2.1.0.dev0` and allowed Qwen3.5/Qwen3.6 conversion to work. Do not casually upgrade or downgrade this environment. First capture versions with:

```cmd
where python.exe
where optimum-cli.exe
python --version
python -m pip show transformers optimum-intel optimum openvino openvino-genai huggingface-hub accelerate
```

### Confirmed OVMS conversions

Qwen3-0.6B language model:

```cmd
C:\Example\OpenVINO\ovms.exe --pull ^
  --task text_generation ^
  --pipeline_type LM ^
  --source_model Qwen/Qwen3-0.6B ^
  --model_repository_path C:\Example\Models ^
  --weight-format int8 ^
  --overwrite_models ^
  --log_level DEBUG ^
  --log_path C:\Example\OpenVINO\qwen3-06b-pull.log
```

Confirmed output:

```text
Model: Qwen/Qwen3-0.6B downloaded to: C:\Example\Models\Qwen\Qwen3-0.6B
Graph: graph.pbtxt created in: C:\Example\Models\Qwen\Qwen3-0.6B
```

Qwen3.5-4B vision-language model:

```cmd
C:\Example\OpenVINO\ovms.exe --pull ^
  --task text_generation ^
  --pipeline_type VLM ^
  --source_model Qwen/Qwen3.5-4B ^
  --model_repository_path C:\Example\Models ^
  --weight-format int8 ^
  --overwrite_models ^
  --log_level DEBUG ^
  --log_path C:\Example\OpenVINO\qwen35-4b-pull-global.log
```

Confirmed output:

```text
Model: Qwen/Qwen3.5-4B downloaded to: C:\Example\Models\Qwen\Qwen3.5-4B
Graph: graph.pbtxt created in: C:\Example\Models\Qwen\Qwen3.5-4B
```

Qwen3.6-27B vision-language model:

```cmd
C:\Example\OpenVINO\ovms.exe --pull ^
  --task text_generation ^
  --pipeline_type VLM ^
  --source_model Qwen/Qwen3.6-27B ^
  --model_repository_path C:\Example\Models ^
  --weight-format int8 ^
  --overwrite_models ^
  --log_level DEBUG ^
  --log_path C:\Example\OpenVINO\qwen36-27b-pull-global.log
```

Current state: the user confirmed the converted Qwen3.6-27B model loads and runs successfully. Treat `C:\Example\Models\Qwen\Qwen3.6-27B` as a known-good local OVMS/OpenVINO conversion unless a later test proves otherwise.

Although Hugging Face may label these models as image-text-to-text, the working OVMS pull combination for the VLM conversions is:

```text
--task text_generation
--pipeline_type VLM
```

### Failed dependency paths

- Older Transformers failed with `KeyError: 'qwen3_5'` / "Transformers does not recognize this architecture."
- A partial upgrade failed with `ImportError: cannot import name 'Qwen3_5DynamicCache' from transformers.models.qwen3_5.modeling_qwen3_5`.
- The cause was a Transformers / Optimum Intel API mismatch. The working fix was `transformers 5.2.0` plus `optimum-intel 2.1.0.dev0` from main.
- `Qwen/Qwen3.6-27B-FP8` was the wrong test path for the INT8 conversion. Do not return to the FP8 source unless there is a separate reason to support an already-quantized FP8 checkpoint.
- The experimental `C:\Example\OpenVINO\venvs\qwen35` environment was not the fix. It used incompatible/yanked package combinations and must not be recreated as the solution.
- Do not copy a random `python312.dll` beside `ovms.exe`. The OVMS distribution already includes the correct embedded Python runtime at `C:\Example\OpenVINO\python`.

### Current OVMS Pull tab behavior

- The old Optimum Intel tab is removed.
- `WorkStationC#.csproj` no longer packages `Runtime\optimum-exporter`.
- OVMS Pull opens a real `cmd.exe` window and runs a generated `.cmd` script.
- The generated script prepends `C:\Example\OpenVINO\python` so OVMS finds its Python 3.12 DLL.
- For non-GGUF Hugging Face export pulls, the generated script also prepends the global Python `Scripts` folder and parent folder so OVMS finds the same `optimum-cli.exe` stack that worked from Command Prompt.
- The OVMS Pull tab has a `Check Stack` button. It compares the live machine against the known-good stack before conversion and prints concrete repair commands when versions do not match.
- `Start Pull` runs the stack check before opening the conversion CMD window for non-GGUF exports. If the version stack is wrong, it stops and prints the fix.
- Source model, task, pipeline type, weight format, model folder, log path, overwrite, and debug logging are visible controls.
- `Source type` is a dropdown. `HF export` uses `--pipeline_type` and `--weight-format`. `GGUF` uses `--gguf_filename`.
- GGUF pulls have a separate `GGUF filename` field. Example:

```cmd
C:\Example\OpenVINO\ovms.exe --pull ^
  --source_model "unsloth/Qwen3.6-27B-MTP-GGUF" ^
  --gguf_filename "Qwen3.6-27B-UD-Q8_K_XL.gguf" ^
  --model_repository_path "C:\Example\Models" ^
  --task text_generation ^
  --overwrite_models ^
  --log_level DEBUG ^
  --log_path "C:\Example\OpenVINO\qwen36-27b-mtp-gguf-q8.log"
```

- GGUF direct pull does not use Optimum/Transformers export packages, so the app skips global `optimum-cli` checks for GGUF mode.
- Future source workflows for `.onnx`, `.pt`, `.bin`, `.gguf`, TensorFlow SavedModel/`.pb`, and Paddle `.pdmodel`/`.pdiparams` belong in OVMS Pull or a separate source-converter path, not in the Convert tab.

## Model storage and compiled blob rules

- The configured main model folder must contain model folders only. Do not create app-owned root helper folders such as `downloads`, `direct`, `local`, `converted`, `compiled-cache`, or `results` inside the main model folder.
- Downloads, including direct web-file downloads, must use `MainModelFolder\<model-name>\main\...`.
- App bookkeeping such as `workbench.cfg` and local-model index files belongs beside the app/project, not inside the main model folder.
- Benchmark, chart, and image test results are not saved to disk by default. Do not recreate a `results` folder unless the user explicitly asks for saved reports.
- The Convert tab is the only app-owned place that should create/export new OpenVINO compiled `.blob` files.
- Workstation and Tests may import and run existing compiled `.blob` files, but they must not auto-create compiled-cache blobs.
- Keep compiled blobs with the specific model they belong to; do not create new root-level blob cache folders.
- New Convert exports must create `MainModelFolder\<model-name>\<target>\model.blob` beside the model's `main` folder, such as `MainModelFolder\<model-name>\main\...` plus `MainModelFolder\<model-name>\CPU\model.blob`, `...\GPU\model.blob`, or `...\MULTI\model.blob`. Do not create `MainModelFolder\<optimized-model-name>\...`, `MainModelFolder\converted\...`, or any other separate converted/cache folder for new exports.
- Convert is now `OpenVINO IR to blob`, not GGUF conversion.
- The visible Convert tab should list existing OpenVINO `.xml/.bin` entries from the model root, include Refresh Models, keep `.blob` selected, and use one `Create Blob` action.
- Convert must not download GGUF, list GGUF as a conversion source, move GGUF files, or convert GGUF to `.xml/.bin`.
- GGUF pulls now belong on OVMS Pull with a Hugging Face repo plus a separate `--gguf_filename`.
- Native OpenVINO IR text-model downloads must keep tokenizer/detokenizer files beside the model when provided, such as `openvino_tokenizer.xml` and `openvino_detokenizer.xml`.
- As soon as the user selects a model folder, Workstation must read the model's local config files and classify the intended run path before Load Model is clicked. Use authoritative local files such as `config.json`, `model_index.json`, `generation_config.json`, `tokenizer_config.json`, `processor_config.json`, and `preprocessor_config.json`, plus OpenVINO file layout, to distinguish text generation, vision-language, text-to-image, image-to-image, image classification, object detection, speech, embeddings, generic Core IR, and compiled blobs. Do not send image-generation or other non-chat models through the chat LLM loader just because an XML file exists.
- OpenVINO Core `MULTI:<devices>` and OpenVINO GenAI LLM multi-GPU loading are not the same execution path. Core IR models such as MobileNet can load directly on `MULTI:GPU.1,GPU.2`, but GenAI LLMs should use one `LLMPipeline`/`VLMPipeline` with the OpenVINO split-model path: `HETERO:GPU.1,GPU.2` plus `MODEL_DISTRIBUTION_POLICY=PIPELINE_PARALLEL`. Keep the UI Workstation target as `MULTI:GPU.1,GPU.2` for the user, but translate it internally for GenAI loads. Do not replace this with an app-level round-robin worker-per-GPU workaround unless the user explicitly asks for a separate serving mode.
- When `.blob` is checked, Convert must use the Workstation tab's selected `Models target:` value.
- `.blob` output remains target/runtime-specific and must be written beside the model's `main` folder as `MainModelFolder\<model-name>\<target>\model.blob`.
- `.onnx`, `.pb`, `.caffemodel`, `.tflite`, `.pdmodel`, `.safetensors`, PyTorch checkpoints, support files, TensorRT engines, and training/data formats are not part of the simplified Convert tab.
- The old Load Model local-library UI and the old Optimum Intel tab are removed. Do not bring either back unless the user explicitly asks.
- The previous bundled `Runtime\optimum-exporter` helper was removed. OVMS Pull uses the installed OVMS/global-Python stack and checks versions before non-GGUF exports.

## Model browsing and download source rules

- Downloads is intentionally KISS: a user-editable Web Address text box plus a Download button are the primary model download workflow.
- The user can paste a Hugging Face model page, tree page, or file page, such as `https://huggingface.co/Qwen/Qwen3-Coder-Next-FP8/tree/main`, and click Download. The app should download the files under that repo/tree into the configured main model folder.
- The Downloads tab has a Find button that checks the pasted Web Address before downloading. For Hugging Face tree/model/file addresses it should confirm the repo, revision, and matching file count; for direct file links it should confirm that the URL responds before saving anything.
- The Downloads tab has twelve programmable saved-address buttons. Clicking one fills the Web Address box inside the app; it must not open the default browser.
- Saved addresses are saved with a single Save Address action: the user enters Web Address and Button Label, chooses a Save To slot, and clicks Save Address.
- The main model folder and the twelve favorite website labels/URLs are persisted in `workbench.cfg` beside the project/app config. `model-root.txt` is legacy and must not be recreated as the active config file.
- Hugging Face downloads go directly under `MainModelFolder\<model-name>\main\...`, so Workstation can select the model folder and inspect its `main` contents.
- Interrupted model downloads must be restart-friendly. When a target file already exists and the authoritative remote size matches the local size, skip that file and continue downloading the missing or mismatched files. Temporary `.partial` files are not considered complete downloads.
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
