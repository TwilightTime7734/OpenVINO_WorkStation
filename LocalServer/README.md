# Open WebUI connection

Load a model in Workstation, then start the server. In Open WebUI, add an OpenAI-compatible connection with base URL `http://localhost:8000/v3`. `/v1` works too. The server does not require an API key; use `unused` if the client requires a value. Select the model returned by the connection's model list.

That address assumes Open WebUI runs on the same Windows host. The app currently listens on IPv4 loopback. A Docker/WSL installation must have a working route to that Windows listener; container `localhost` addresses the container itself.

For image analysis, load a vision-language model and enable **Vision** in Open WebUI's model capabilities. Attach a PNG or JPEG to a chat message. Text-only models return a clear HTTP 400 for image input.

## Implemented endpoints

| Method | OVMS-compatible route | Existing alias |
| --- | --- | --- |
| GET | `/v3/models` | `/v1/models` |
| GET | `/v3/models/{model}` | `/v1/models/{model}` |
| POST | `/v3/chat/completions` | `/v1/chat/completions` |
| POST | `/v3/completions` | `/v1/completions` |

Starting the server displays and logs the API base URL `http://localhost:8000/v3`.

`GET /v2/health/live` and `GET /v2/health/ready` use OVMS health routes and return HTTP 200 with an empty body while the server is serving requests. These report server readiness, independently of whether a model is loaded. `GET /health` still reports detailed server and model state, including the API base URL and primary endpoints. `OPTIONS` supports browser preflight requests.

Both completion endpoints support unary JSON and `stream: true`. Streaming delivers native text deltas during generation, flushes each SSE event, sends the finish chunk, and terminates with `data: [DONE]`. Chat chunks use `choices[].delta`; text chunks use `choices[].text`. Disconnecting cancels active or queued generation. Errors before streaming retain their HTTP status; failures after streaming begins abort the response rather than sending a successful ending.

`stream_options.include_usage: true` adds a final empty-choices chunk with native token counts. The server honors `max_tokens`/`max_completion_tokens` and `temperature`. The default output limit is 2048 tokens. LLM chat preserves message roles and supports `chat_template_kwargs.enable_thinking`/`preserve_thinking`, plus the existing top-level aliases for Qwen templates. Generated reasoning tags and whitespace remain intact in both unary and streamed output.

Image input uses OpenAI content parts: `{"type":"image_url","image_url":{"url":"data:image/png;base64,..."}}`. HTTP(S) image URLs also work. There are limits of eight images per request, 20 MiB per encoded image, 40 million pixels per image, and 32 MiB for the complete JSON request. Images are disposed after generation, including canceled and failed requests.

This is the workstation's single-model chat server, not complete OVMS protocol coverage. Image generation/editing remain HTTP 501. Embeddings, reranking, audio, KServe/gRPC, and native tool-call/structured-output parsers are not implemented. The bundled C runtime's VLM history entry point is unsupported, so VLM chat uses a role-labeled transcript with ordered image markers; LLM chat uses structured history. VLM-specific chat-template options are not exposed by that C entry point.

## Verification

Run `dotnet run --project Checks/WorkstationChecks.csproj` for HTTP streaming, disconnects, protocol errors, image decoding, and existing workstation checks. The streaming fixture prevents generation from finishing until the client has read real text, so buffering regressions cannot pass merely by returning `[DONE]`.

Append a model directory and device to also test native HTTP generation, multiple streaming chunks, token usage, and image recognition when the model supports vision. Replace the example path and device with your own:

```powershell
dotnet run --project Checks/WorkstationChecks.csproj -- '<model-directory>' 'GPU.0'
```

References: [OVMS clients and /v3 endpoints](https://docs.openvino.ai/2026/model-server/ovms_docs_clients_genai.html), [OVMS health endpoints](https://docs.openvino.ai/2026/model-server/ovms_docs_rest_api_kfs.html), [Open WebUI compatible connections](https://docs.openwebui.com/getting-started/quick-start/connect-a-provider/starting-with-openai-compatible/), [Open WebUI model capabilities](https://docs.openwebui.com/features/workspace/models/).

## Local model template repair

On 2026-09-07, the user authorized repairing the native template error in the installed Qwen3.8 exports. The unsupported Jinja test `is undefined` was changed to its equivalent `is not defined` in sixteen files across these six model directories:

- `Qwen3.8-27B-obliterated-ov`
- `Qwen3.8-27B-obliterated-int4-int8-20-80-ov`
- `Qwen3.8-27B-obliterated-int4-int8-25-75-ov`
- `Qwen3.8-27B-obliterated-int4-int8-40-60-ov`
- `Qwen3.8-27B-int8-ov\main`
- `Qwen3.8-27B-INT4-INT8-50-50-OV` (detokenizer metadata)

The edits cover each affected `chat_template.jinja` and embedded `chat_template` in `tokenizer_config.json`, `openvino_tokenizer.xml`, and `openvino_detokenizer.xml`. The native tokenizer reads the XML metadata, so repairing only the sidecar files does not fix native loading. Rendering checks confirmed identical output for text, images, thinking options, tool definitions, and tool-result history. This is an on-disk model repair; the app does not rewrite templates at startup.

Native GenAI 2026.3.0 tokenizer checks passed for the original obliterated model, its three quantized variants, and the 50-50 export. The separate int8/main tokenizer process exited with an access violation before rendering; that export has not been verified by this check.
