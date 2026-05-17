# GroupDocs.Parser.Mcp.Tests

Integration tests for the [`GroupDocs.Parser.Mcp`](https://github.com/groupdocs-parser/GroupDocs.Parser.Mcp)
MCP server — exposes [GroupDocs.Parser](https://products.groupdocs.com/parser)
as AI-callable tools.

This repository validates the **published** distribution end-to-end: it
launches the server, connects as an MCP client, and exercises every advertised
tool against real document fixtures sourced from the upstream
[GroupDocs.Parser-for-.NET Examples](https://github.com/groupdocs-parser/GroupDocs.Parser-for-.NET).

> **NOTE — Docker-first distribution**: as of 26.5.0, `GroupDocs.Parser.Mcp`
> is shipped **only via Docker** because the engine DLL (~235 MB) exceeds
> NuGet.org's 250 MB package-size limit. The `dnx GroupDocs.Parser.Mcp@26.5.0
> --yes` route is currently blocked. Integration tests target the Docker
> image at `ghcr.io/groupdocs-parser/parser-net-mcp:26.5.0`. See the main
> repo's `changelog/001` for context.

## Documentation

- [how-to/](how-to/) — step-by-step guides for every deployment channel.
- [examples/](examples/) — ready-to-paste `claude-desktop.json`,
  `vscode-mcp.json`, and `docker-compose.yml`.
- [AGENTS.md](AGENTS.md) — orientation for AI coding agents working in this repo.
- [llms.txt](llms.txt) — machine-readable summary for LLM tooling.
- [changelog/](changelog/) — one entry per change set.
- [Files/README.md](Files/README.md) — provenance of real fixtures (which
  MCP method ↔ which upstream example ↔ which file).

## What gets tested

| Area | Covered by |
|---|---|
| Package installs and starts | [McpServerFixture](src/GroupDocs.Parser.Mcp.Tests/Fixtures/McpServerFixture.cs) |
| MCP handshake, server info, tool list (6 tools) | [ToolDiscoveryTests](src/GroupDocs.Parser.Mcp.Tests/ToolDiscoveryTests.cs) |
| `extract_text` — PDF / DOCX / EPUB / HTML, page parameter, password | [ExtractTextTests](src/GroupDocs.Parser.Mcp.Tests/ExtractTextTests.cs) |
| `extract_images` — PDF + XLSX with embedded images, synthetic empty | [ExtractImagesTests](src/GroupDocs.Parser.Mcp.Tests/ExtractImagesTests.cs) |
| `extract_metadata` — DOCX / PDF, valid JSON schema | [ExtractMetadataTests](src/GroupDocs.Parser.Mcp.Tests/ExtractMetadataTests.cs) |
| `extract_tables` — invoice PDF, Markdown + JSON formats | [ExtractTablesTests](src/GroupDocs.Parser.Mcp.Tests/ExtractTablesTests.cs) |
| `extract_barcodes` — barcodes PDF, JSON schema validation | [ExtractBarcodesTests](src/GroupDocs.Parser.Mcp.Tests/ExtractBarcodesTests.cs) |
| `get_document_info` — synthetic + 5 real samples, JSON shape | [GetDocumentInfoTests](src/GroupDocs.Parser.Mcp.Tests/GetDocumentInfoTests.cs) |
| Unknown / corrupted files, password parameter | [ErrorHandlingTests](src/GroupDocs.Parser.Mcp.Tests/ErrorHandlingTests.cs) |

## Running locally

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
dotnet test
```

> **Note**: while the NuGet-publish path is blocked (see above), the test
> fixture would launch the server via `dnx` against the published NuGet —
> which won't resolve. Tests currently exercise the code paths but require
> infrastructure changes (Docker-based test fixture) to run end-to-end.
> Track as a follow-up item; the test suite itself is structurally complete.

## CI

[.github/workflows/integration.yml](.github/workflows/integration.yml) runs
on push / PR / nightly cron / `workflow_dispatch` / `repository_dispatch`.
Matrix: `ubuntu-latest`, `windows-latest`, `macos-latest`. Linux runners
install `libgdiplus` + `libfontconfig1`.

## Evaluation vs licensed mode

In evaluation mode (no `GROUPDOCS_LICENSE_PATH`), Parser may truncate text
output, watermark some responses, or limit row counts. Tests pass either way
— assertions check JSON-shape validity and the absence of the per-tool
`<Operation> failed for ...` diagnostic prefix, not exact text contents.

For CI, store a base64-encoded `.lic` file as repo secret `GROUPDOCS_LICENSE`
— the workflow decodes it into `$RUNNER_TEMP` and exports
`GROUPDOCS_LICENSE_PATH` automatically.

## Fixture documents

Real fixtures live under [Files/](Files/) and are sourced from the upstream
`GroupDocs.Parser-for-.NET/Examples/Resources/SampleFiles/` folder. Each
fixture is documented in [Files/README.md](Files/README.md) with its MCP
method ↔ upstream example ↔ filename mapping.

## License

MIT — see [LICENSE](LICENSE)
