# How-to guides

Step-by-step guides for verifying and using every deployment channel of
[`GroupDocs.Parser.Mcp`](https://www.nuget.org/packages/GroupDocs.Parser.Mcp).

Each guide is self-contained — pick the one that matches your workflow. They
all point at the same published artifact (`26.4.4` at time of writing).

| # | Guide | When to use |
|---|---|---|
| 01 | [Install from NuGet (dnx + dotnet tool)](01-install-from-nuget.md) | You have the .NET 10 SDK. Fastest path — no Docker required. |
| 02 | [Run via Docker](02-run-via-docker.md) | You'd rather not install .NET, or want isolation from the host. |
| 03 | [Verify on the MCP registry](03-verify-mcp-registry.md) | You want to confirm the package shows up in MCP clients' discovery UIs and that its `server.json` metadata is correct. |
| 04 | [Use with Claude Desktop](04-use-with-claude-desktop.md) | Connect from Claude Desktop (macOS / Windows). |
| 05 | [Use with VS Code / GitHub Copilot](05-use-with-vscode-copilot.md) | Connect from VS Code's MCP support or GitHub Copilot agents. |
| 06 | [Run the integration tests](06-run-integration-tests.md) | Validate a specific published version end-to-end; set up CI. |

## Which guide first?

- **Trying the server for the first time** → start with
  [01 — NuGet via dnx](01-install-from-nuget.md). One command, no install.
- **Debugging a broken release** → [06 — Integration tests](06-run-integration-tests.md),
  then cross-check with [03 — MCP registry](03-verify-mcp-registry.md).
- **Wiring an AI agent to production documents** → pick your client:
  [04 — Claude Desktop](04-use-with-claude-desktop.md) or
  [05 — VS Code](05-use-with-vscode-copilot.md).

## Common context

- All guides target `GroupDocs.Parser.Mcp@26.5.0`. Substitute a newer version
  freely — the interfaces haven't changed.
- Tools exposed on the wire (snake_case): `extract_text`, `extract_images`,
  `extract_metadata`, `extract_tables`, `extract_barcodes`, `get_document_info`.
- **As of 26.5.0, distribution is Docker-only** (the NuGet path is blocked by
  the 250 MB package-size limit on the engine DLL). Guides 01 and 03 reference
  the NuGet/MCP-registry path and currently don't apply to Parser.
- All 6 Parser tools work in evaluation mode without a license. Text outputs
  may be truncated and some responses may carry an `[Evaluation mode] Output
  may be limited.` banner. Setting `GROUPDOCS_LICENSE_PATH` suppresses these.
