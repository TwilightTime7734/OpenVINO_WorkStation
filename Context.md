# WorkStationC# - Codex Project Context

This file is the working source of truth for future Codex/ChatGPT sessions. Keep it clean, current, and practical. Remove stale notes instead of adding contradictory layers.

## Current state - 2026-08-22

- The project is a .NET 10 C# WinForms OpenVINO WorkStation/Workbench application.
- The previous `Tests` tab and `Server Tests` tab have been removed from the UI.
- The `Tests` and `ServerTests` folders are currently excluded from the project but have not been deleted yet. Leave them excluded until the refactor is stable; delete them only when the user explicitly decides the cleanup is complete.
- After removing the two test tabs and most test-only code, `Form1.cs` is still roughly 11,000 lines. The project rebuilds with 0 errors; two existing unreachable-code warnings may remain during the refactor.
- The current refactor goal is to return the application to clear object-oriented structure: real classes for models/state/services/controllers, enums for fixed categorical values, and thin WinForms event handlers in `Form1.cs`.
- Refactor the old `Common` region first so shared state/types have an authoritative home before tab code is extracted.
- Refactor incrementally. After each meaningful extraction, rebuild before moving to the next area.

## Non-negotiable WinForms Designer and event rules

These rules must never be removed, shortened away, or overridden during context updates:

- `Form1.Designer.cs` owns UI design: control creation, layout, sizing, labels, colors, fonts, anchors, docking, tab order, and named event hookups.
- `Form1.cs` owns the actual `Form1 : Form` code-behind: constructor/bootstrap, named event handlers, shared form behavior, tab behavior, and runtime plumbing that still genuinely belongs to the form.
- Do not create new `MainForm.*.cs` or other `partial Form1` behavior files. Visual Studio Community previously classified additional partial `Form1` files as Designer Forms and wrote bad `WorkStationC#.csproj.user` metadata.
- Large behavior must move into normal non-Form helper/controller/service classes that do not inherit from `Form` and are not `partial class Form1`.
- Keep Designer event hookups named, with thin named handlers in `Form1.cs` delegating to the appropriate class.
- `WorkStationC#.csproj.user` is per-user Visual Studio state and must not be treated as project source. It is ignored by `.gitignore`; if Visual Studio recreates it with stale form metadata, close Visual Studio and delete the `.csproj.user` file.
- Every user-interface control event must use a named event-handler method implemented in `Form1.cs`.
- Do not use lambdas or anonymous delegates for control events such as `Click`, `TextChanged`, `SelectedIndexChanged`, drag/drop, keyboard, tab, drawing, or similar events.
- Designer controls must be wired by name in `Form1.Designer.cs`, for example `btnLoad.Click += btnLoad_Click;`.
- Double-clicking a control or opening its event in Visual Studio Properties must lead to the real named handler in `Form1.cs`; it must not create an empty duplicate handler.
- Runtime code must not rename, reposition, resize, hide, or restyle Designer-owned controls unless the behavior truly has to be dynamic. If a runtime visual change is unavoidable, add a short comment explaining why.
- Before completing any UI change, search for newly introduced anonymous control-event handlers and reject the change if any were added.
- Designer event-handler compliance command:

```text
rg -n "\.(Click|TextChanged|SelectedIndexChanged|DragDrop|DragEnter|DragLeave|KeyDown|Leave|DrawItem|DrawColumnHeader|DrawSubItem|Selecting)\s*\+=\s*(async\s*)?\(" Form1.Designer.cs Form1.cs
```

The required result is zero anonymous control-event matches.

## Non-negotiable Windows line-ending rule

- All repository text files must use Windows `CRLF` line endings.
- New files must be created with `CRLF`, and edited files must retain or be normalized to `CRLF` before the work is completed.
- Do not leave LF-only or mixed line endings in any file touched by a change.
- Verify line endings for every text file changed.
- This project is maintained with Visual Studio Community on Windows; non-Windows line endings have caused editor, Designer, build, and source-control trouble.

## Non-negotiable root-cause and source-of-truth rule

This rule must never be removed, shortened away, or overridden during context updates:

- Never hide, bypass, compensate for, or patch around an error merely to make the symptom disappear.
- Find and correct the actual cause at its authoritative source of truth.
- Do not add startup fixups, duplicate state, fallback UI mutations, catch-and-ignore behavior, or special-case workarounds when the underlying Designer definition, model state, path resolver, configuration, or core logic can be corrected directly.
- Do not create duplicate mirror fields merely to preserve the old `Form1` shape during refactoring. Move the real state to its new class and update callers to use that class.
- If the source of truth cannot be identified or safely changed, stop and report that clearly instead of applying a workaround without the user's explicit approval.
- After correcting the source, remove obsolete patches and verify the original failing path directly.

## OOP refactor direction

The project should use straightforward, readable object-oriented C# rather than a giant form containing unrelated loose fields and methods.

Preferred structure:

```text
Models/
    ModelEnums.cs
    WorkbenchModel.cs
    WorkbenchState.cs

Runtime/
    OpenVinoRuntimeSupport.cs

Workstation/
    WorkstationMain.cs

Downloads/
    DownloadsMain.cs

OVMS_Pull/
    OVMS_PullMain.cs

Convert/
    ConvertMain.cs

LocalServer/
    LocalServerMain.cs

Form1.cs
Form1.Designer.cs
```

Guidelines:

- Use classes/properties (`get`/`set`) for real shared concepts such as a selected model, loaded model, workbench state, device information, conversion job, or download job.
- Use enums for fixed categorical values such as model file kind, run path, status, or capabilities where the value set is truly closed.
- Do not use enums for dynamic OpenVINO device names such as `GPU.0`, `GPU.1`, `AUTO`, `HETERO:...`, or `MULTI:...`; those remain strings or become a `DeviceInfo` class.
- Prefer composition over large inheritance trees.
- The model itself should become a real object instead of a collection of unrelated fields such as `_selectedRuntimeModelPath`, `_loadedRuntimeModelKind`, and `_loadedModelAcceptsText` scattered through `Form1`.
- `WorkbenchState` should be the authoritative owner of shared application state once introduced.
- Do not keep two competing representations of the same state.
- Pure model/path/classification helpers should move out of `Form1` when they no longer need controls.
- OpenVINO runtime helpers shared by multiple areas belong in `Runtime/OpenVinoRuntimeSupport.cs` or a similarly focused normal class.
- UI-only helpers that directly manipulate controls may remain in `Form1` until there is a clean controller boundary.
- Each extracted tab class should receive the state/services it actually needs; it should not reach through `Form1` for dozens of unrelated controls.
- Named event handlers stay in `Form1.cs` and should become thin delegates such as `btnCreateBlob_Click -> _convertMain.CreateBlobAsync(...)` once the controller exists.

Refactor order:

1. Common shared enums/data/state classes.
2. Common pure helpers and shared OpenVINO runtime helpers.
3. Convert.
4. OVMS Pull.
5. Downloads.
6. Local Server.
7. Workstation last because it currently owns the most shared/runtime behavior.

The exact order after Common may change if dependencies make another order safer, but Common/state comes first and Workstation should remain late.

## Plain-language UI rule

- User-facing labels, buttons, status messages, and Help must use ordinary language that a non-developer can understand.
- Avoid developer-only terms such as `smoke test`, `tensor`, `pipeline`, `runtime entry`, and `inference request` in normal controls or instructions.
- Prefer familiar actions such as `Send Message`, `Load Model`, `Create Blob`, `Working`, and `Cancel`.
- The old Tests-tab-specific UI wording is obsolete because the Tests tab no longer exists. Do not reintroduce a Tests tab or Server Tests tab unless the user explicitly asks.
- Before completing a UI change, review every new visible string from the perspective of a first-time user.

## Project purpose

This project is a C# WinForms OpenVINO workstation/workbench. It is not meant to be a line-by-line translation of the earlier Python project.

The normal Workstation, Convert-to-blob, Downloads, OVMS Pull, and local-server workflows should feel self-contained to the user. Runtime dependencies should be bundled deliberately when practical.

OVMS `--pull` export is intentionally different: it depends on the user's installed `C:\Example\OpenVINO\ovms.exe`, the OVMS bundled Python DLL folder, and a known-good global Python export stack. Do not pretend this path is fully standalone. The app should check that stack, explain mismatches plainly, and recommend exact repair commands before starting a non-GGUF pull.

## Hardware and architecture assumptions

- Windows host.
- Two Intel Arc Pro B70 GPUs, 32 GB VRAM each.
- The display is forced to CPU/integrated graphics in BIOS so the B70 VRAM is available.
- The old round-robin model-loading idea was removed. Do not reintroduce it.
- Multi-GPU support is not the current refactor focus; do not let it complicate unrelated cleanup.
- When multi-GPU GenAI is used, OpenVINO Core `MULTI:<devices>` and OpenVINO GenAI split-model loading are different execution paths.
- Core IR models can use OpenVINO Core device strings directly.
- GenAI LLM/VLM split-model loading uses one `LLMPipeline`/`VLMPipeline` with the required OpenVINO translation from the UI multi-device target to `HETERO:GPU.1,GPU.2` plus `MODEL_DISTRIBUTION_POLICY=PIPELINE_PARALLEL` when that working path is selected. Do not replace that with an app-level worker-per-GPU round-robin workaround.

## Model types and runtime rules

- Model selection must classify the intended run path from authoritative local model files and layout before loading.
- Use local files such as `config.json`, `model_index.json`, `generation_config.json`, `tokenizer_config.json`, `processor_config.json`, and `preprocessor_config.json`, plus OpenVINO file layout, to distinguish text generation, vision-language, text-to-image, image-to-image, image classification, object detection, speech, embeddings, generic Core IR, and compiled blobs.
- Do not send image-generation or other non-chat models through the chat LLM loader merely because an XML file exists.
- The installed OpenVINO GenAI C runtime exports VLM `generate`, `start_chat`, and `finish_chat`, but not `ov_genai_vlm_pipeline_generate_with_history`.
- VLM text/image paths must call `VLMPipeline.Generate(...)`, not `VLMPipeline.GenerateWithHistory(...)`, unless a future runtime update is verified to export the history function.
- `LLMPipeline.GenerateWithHistory(...)` remains valid for language-only models.
- VLM image tensors must be unsigned 8-bit RGB with a batch dimension in NHWC order: `1 x height x width x 3`.
- Do not pass bare `height x width x 3` tensors to GenAI VLM image generation.

## OVMS Pull and known-good export stack

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
- During `ovms.exe --pull`, OVMS launches the `optimum-cli.exe` found on `PATH`.
- The successful Qwen conversions used the global Python 3.14 installation and its global `optimum-cli.exe`.
- This is intentional: OVMS loads `python312.dll`, while the export subprocess runs through global Python 3.14.

Known-good global export stack:

```text
Python             3.14.6
transformers       5.2.0
optimum-intel      2.1.0.dev0
optimum            2.2.0
openvino           2026.2.1
huggingface-hub    1.21.0 (query live before treating Hub as exact)
accelerate         1.14.0
```

Critical Optimum Intel repair used during troubleshooting:

```cmd
"C:\Users\ExampleUser\AppData\Local\Python\pythoncore-3.14-64\python.exe" -m pip install --upgrade --force-reinstall --no-deps "https://github.com/huggingface/optimum-intel/archive/refs/heads/main.zip"
```

Do not casually upgrade or downgrade this environment. First capture versions with:

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

The converted `C:\Example\Models\Qwen\Qwen3.6-27B` model was confirmed to load and run successfully.

Although Hugging Face may label these models as image-text-to-text, the working OVMS pull combination for the VLM conversions is:

```text
--task text_generation
--pipeline_type VLM
```

### Failed dependency paths to avoid

- Older Transformers failed with `KeyError: 'qwen3_5'` / architecture-not-recognized errors.
- A partial upgrade failed with `ImportError: cannot import name 'Qwen3_5DynamicCache'`.
- The root cause was a Transformers / Optimum Intel API mismatch.
- The working fix was `transformers 5.2.0` plus `optimum-intel 2.1.0.dev0` from main.
- `Qwen/Qwen3.6-27B-FP8` was the wrong path for the INT8 conversion test; do not return to it unless supporting an already-quantized FP8 checkpoint is an explicit goal.
- The experimental `C:\Example\OpenVINO\venvs\qwen35` environment was not the fix and must not be recreated as the solution.
- Do not copy a random `python312.dll` beside `ovms.exe`; the OVMS distribution already includes the correct embedded Python runtime under `C:\Example\OpenVINO\python`.

### Current OVMS Pull behavior

- The old Optimum Intel tab is removed.
- `WorkStationC#.csproj` no longer packages `Runtime\optimum-exporter`.
- OVMS Pull opens a real `cmd.exe` window and runs a generated `.cmd` script.
- The generated script prepends `C:\Example\OpenVINO\python` so OVMS finds its Python 3.12 DLL.
- For non-GGUF Hugging Face export pulls, the generated script also prepends the global Python `Scripts` folder and parent folder so OVMS finds the known-good `optimum-cli.exe` stack.
- The OVMS Pull tab has a `Check Stack` button.
- `Start Pull` runs the stack check before opening the conversion CMD window for non-GGUF exports.
- `Source type` is a dropdown. `HF export` uses `--pipeline_type` and `--weight-format`. `GGUF` uses `--gguf_filename`.
- GGUF direct pull does not use Optimum/Transformers export packages, so the app skips global `optimum-cli` checks for GGUF mode.
- Future source workflows for `.onnx`, `.pt`, `.bin`, `.gguf`, TensorFlow SavedModel/`.pb`, and Paddle `.pdmodel`/`.pdiparams` belong in OVMS Pull or a separate source-converter path, not in the Convert tab.

## Model storage and compiled blob rules

- The configured main model folder must contain model folders only.
- Do not create app-owned root helper folders such as `downloads`, `direct`, `local`, `converted`, `compiled-cache`, or `results` inside the main model folder.
- Downloads, including direct web-file downloads, use `MainModelFolder\<model-name>\main\...`.
- App bookkeeping such as `workbench.cfg` and local-model index files belongs beside the app/project, not inside the main model folder.
- The Convert tab is the only app-owned place that should create/export new OpenVINO compiled `.blob` files.
- Workstation may import and run existing compiled `.blob` files but must not auto-create compiled-cache blobs.
- Keep compiled blobs with the specific model they belong to; do not create root-level blob cache folders.
- New Convert exports create `MainModelFolder\<model-name>\<target>\model.blob` beside the model's `main` folder.
- Convert is `OpenVINO IR to blob`, not GGUF conversion.
- The visible Convert tab should list existing OpenVINO `.xml/.bin` entries from the model root, include Refresh Models, keep `.blob` selected, and use one `Create Blob` action.
- Convert must not download GGUF, list GGUF as a conversion source, move GGUF files, or convert GGUF to `.xml/.bin`.
- GGUF pulls belong on OVMS Pull with a Hugging Face repo plus a separate `--gguf_filename`.
- Native OpenVINO IR text-model downloads must keep tokenizer/detokenizer files beside the model when provided.
- When `.blob` is selected, Convert uses the Workstation tab's selected `Models target:` value.
- `.onnx`, `.pb`, `.caffemodel`, `.tflite`, `.pdmodel`, `.safetensors`, PyTorch checkpoints, support files, TensorRT engines, and training/data formats are not part of the simplified Convert tab.
- The old Load Model local-library UI and old Optimum Intel tab are removed. Do not bring either back unless the user explicitly asks.

## Downloads rules

- Downloads is intentionally KISS: a user-editable Web Address text box plus a Download button are the primary model-download workflow.
- The user can paste a Hugging Face model page, tree page, or file page and click Download.
- Hugging Face downloads go under `MainModelFolder\<model-name>\main\...` so Workstation can select the model folder and inspect its `main` contents.
- The Downloads tab has a Find button that validates the pasted Web Address before downloading.
- The Downloads tab has twelve programmable saved-address buttons. Clicking one fills the Web Address box inside the app; it must not open the default browser.
- Saved addresses are stored with one Save Address action using Web Address, Button Label, and Save To slot.
- The main model folder and favorite website labels/URLs are persisted in `workbench.cfg` beside the project/app config.
- `model-root.txt` is legacy and must not be recreated as the active config file.
- Interrupted model downloads must be restart-friendly: if the authoritative remote size matches the local size, skip the existing complete file and continue with missing/mismatched files. `.partial` files are not complete downloads.

## Local model API server

The built-in local server is an OpenAI-compatible local API that reuses the Workstation's loaded model state. It is no longer represented by a Server Tests tab.

Current server rules:

- Bind to loopback only: `127.0.0.1` / `localhost` on port `11434`.
- Base URL: `http://localhost:11434`.
- Do not restore the previous `0.0.0.0`, LAN-discovery, or `192.168.0.11` behavior unless the user explicitly asks to expose the server beyond the local machine.
- Workstation owns Start Local Server / Stop Local Server, local server address, and status.
- Reuse the loaded Workstation model state; do not create a separate server-only model picker unless the user explicitly asks.
- Closing the app or stopping the server releases server/model resources as appropriate.

Current API surface:

- `GET /`
- `GET /health`
- `GET /v1/models`
- `POST /v1/chat/completions`
- `POST /v1/completions`
- `POST /v1/images/generations` returns clear not-implemented JSON (HTTP 501).
- `POST /v1/images/edits` returns clear not-implemented JSON (HTTP 501).

Server behavior:

- `/v1/models` reports known models/current loaded model as supported by the current implementation.
- `/v1/chat/completions` and `/v1/completions` use the loaded Workstation text model for supported OpenVINO GenAI/GGUF paths.
- The server does not execute tools. Extra OpenAI-style fields may be accepted/ignored for compatibility, but do not inject tool schemas into prompts or invent structured tool calls until real tool execution exists.
- Streaming, where supported by the compatibility layer, may be produced after the existing model-generation path completes; do not redesign streaming during the current structural refactor.
- Use plain Output messages that explain whether the server is stopped, running, missing a loaded model, or receiving an unsupported request.
- Do not expose the server to the internet unless the user explicitly asks and the risks are discussed.

## Refactor safety/checkpoint rules

- Do not perform another bulk automated method-moving refactor.
- Make one coherent structural change at a time.
- Rebuild after each change.
- If a change creates a large cross-project error cascade, revert that change rather than stacking more patches on top.
- Preserve working runtime behavior while changing structure.
- Do not change OpenVINO device-selection semantics, model-classification semantics, download paths, conversion output paths, or server protocol behavior merely because code is being moved.
- Do not delete the excluded `Tests` or `ServerTests` folders until the refactor is stable and the user explicitly approves deletion.

## What not to do

- Do not publish a final release unless the user asks.
- Do not convert the old Python project in one pass.
- Do not replace WinForms with WPF, MAUI, Avalonia, or another UI framework.
- Do not downgrade from .NET 10.
- Do not make app/runtime code depend on `/Extras`.
- Do not assume a model supports image input; inspect model capability first.
- Do not use hidden runtime patches to fix Designer mistakes; fix `Form1.Designer.cs`.
- Do not reintroduce Tests or Server Tests tabs during cleanup.
- Do not use extra partial `Form1` files as a way to make `Form1.cs` look smaller.

## Communication style

The user is technically capable and wants practical results.

- Be direct.
- Use plain language in the app and in explanations.
- Show exact file names and build errors.
- Avoid vague statements.
- When something cannot be verified, say exactly what remains unverified.
- Prefer incremental implementation over a large speculative rewrite.
