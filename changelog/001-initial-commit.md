---
id: 001
date: 2026-05-15
version: 26.5.0
type: feature
---

# Initial integration-test repo for GroupDocs.Parser.Mcp 26.5.0

## What changed
- Public integration-test repo `groupdocs-parser/GroupDocs.Parser.Mcp.Tests` published.
- Exercises the **published** `GroupDocs.Parser.Mcp@26.5.0` distribution. **Note**: distributed via Docker only for now (the NuGet path is blocked by the 250 MB package-size limit; see main repo's `changelog/001` for context).
- Test suites (covering all 6 advertised tools):
  - `ToolDiscoveryTests` — server handshake, tool listing (asserts exactly 6 tools: `extract_text`, `extract_images`, `extract_metadata`, `extract_tables`, `extract_barcodes`, `get_document_info`), schema sanity.
  - `ExtractTextTests` — text extraction across 4 real samples (PDF / DOCX / EPUB / HTML), page parameter, and password-protected PDF.
  - `ExtractImagesTests` — image extraction from PDF + XLSX with embedded images, plus a "synthetic empty PDF returns no images gracefully" check.
  - `ExtractMetadataTests` — DOCX and PDF metadata, valid JSON schema (Pitfall #16 — returns raw JSON, no `TruncateText`).
  - `ExtractTablesTests` — table extraction from the upstream's canonical invoice PDF, both Markdown (default) and JSON (`format='json'`) paths.
  - `ExtractBarcodesTests` — barcode/QR detection from the upstream's canonical Barcodes.pdf, plus a synthetic-empty check.
  - `GetDocumentInfoTests` — synthetic + 5 real samples, asserts `fileName` / `fileType` / `size` / `pageCount` JSON shape.
  - `ErrorHandlingTests` — unknown filename, corrupted bytes, password parameter accepted by schema.
- **Real fixtures sourced from the upstream Examples repo** (per Step 6e of the clone prompt): 10 files copied from `GroupDocs.Parser-for-.NET/Examples/Resources/SampleFiles/`. Each fixture documented in `Files/README.md` with MCP method ↔ upstream example ↔ filename mapping. Total ~1.3 MB.
- Synthetic-fixture builder writes a minimal bare 1-page PDF at runtime as a "controlled empty" baseline.
- Integration workflow (`.github/workflows/integration.yml`): matrix × 3 OS, nightly cron, release-smoke `repository_dispatch` listener.
- Linux runners install `libgdiplus libfontconfig1` (base apt set per Pitfall #17 tier 1 — Parser doesn't render text glyphs, so MS core fonts are NOT required).

## Pitfall remediations baked in
- **`ToolCatalog` keyword resolvers use snake_case substrings** that uniquely identify each tool (`extract_text`, `extract_images`, `extract_metadata`, `extract_tables`, `extract_barcodes`, `document_info`) — Pitfall #15 audit clean.
- **Test fixtures parse JSON via `JsonDocument.Parse`** rather than `result.IsError == true` — the main server's tools return `<Op> failed for '...'` descriptive error strings, not throws (Pitfall #18). Tests assert `DoesNotContain("<Op> failed for", body)` on success path.
- **Fixtures provenance documented** — `Files/README.md` is the single source of truth for "which upstream example brought in which file." Refresh command included for keeping fixtures up to date with upstream.

## Why
Sixth product Tests repo in the GroupDocs MCP framework family (after Metadata, Conversion, Comparison, Viewer, Watermark). Validates the shipped distribution end-to-end and doubles as a reference for users deploying Parser via Docker (NuGet path currently unavailable for this product).

## Migration / impact
First release — no migration required.

## TODO before publish
- [ ] Adapt `McpServerFixture` to launch the Docker image instead of `dnx` (or document that integration tests run from the Docker image with `docker-compose run`). Currently the fixture uses `dnx` which requires the NuGet package on NuGet.org — not viable for Parser until the package-size constraint is resolved.
- [ ] Once Docker-based fixture lands, re-enable nightly CI runs and verify against the live image tag.
