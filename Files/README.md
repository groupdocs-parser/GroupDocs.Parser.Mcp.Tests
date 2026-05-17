# Files — real document fixtures for the integration suite

Real fixtures used by the integration suite, sourced from the upstream
[GroupDocs.Parser-for-.NET Examples repo](https://github.com/groupdocs-parser/GroupDocs.Parser-for-.NET).

## Provenance

Each fixture is here because an upstream example uses it to demonstrate a
specific Parser operation. The Tests repo exercises the same operation
through the MCP server, so re-using the upstream input keeps the test
inputs aligned with what the engine team has already validated.

| File | MCP method(s) | Upstream example | Notes |
|---|---|---|---|
| `sample.pdf` | `ExtractText`, `GetDocumentInfo` | `BasicUsage/ExtractTextFromDocuments.cs` (`Constants.SamplePdf`) | Multi-page PDF with text — primary text-extraction target |
| `images.pdf` | `ExtractImages` | `BasicUsage/ExtractImagesFromDocuments.cs` (`Constants.SampleImagesPdf`) | PDF with embedded images |
| `images.xlsx` | `ExtractImages` (format variant) | (Constants.SampleWithImagesXlsx) | Office spreadsheet with embedded images |
| `sample.docx` | `ExtractMetadata`, `GetDocumentInfo`, `ExtractText` | `BasicUsage/ExtractMetadataFromDocuments.cs`, `BasicUsage/GetDocumentInfo.cs` (`Constants.SampleDocx`) | DOCX with rich Office metadata |
| `invoice_pages.pdf` | `ExtractTables` | `AdvancedUsage/WorkingWithTables/ExtractTablesFromDocument.cs` (`Constants.SampleInvoicePagesPdf`) | Multi-page PDF with line-item tables |
| `Barcodes.pdf` | `ExtractBarcodes` | `AdvancedUsage/WorkingWithBarcodes/ExtractBarcodesFromDocument.cs` (`Constants.SamplePdfWithBarcodes`) | PDF with multiple barcode formats embedded |
| `samplePassword.pdf` | (`password` parameter on all tools) | `AdvancedUsage/Loading/PasswordProtectedDocuments.cs` (`Constants.SamplePassword`) | Password-protected PDF. **Password: `123456`** |
| `sample.epub` | `ExtractText` (EPUB format coverage) | upstream Resources/SampleFiles | EPUB exercises an alternate text-extraction path |
| `sample.htm` | `ExtractText` (HTML format coverage) | upstream Resources/SampleFiles | HTML exercises another alternate path |
| `corrupted.png` | `ErrorHandlingTests` (corrupted-input test) | upstream Resources/SampleFiles | Intentionally corrupted image — tests must not crash the server |

## Refresh command

To pull the latest versions from upstream:

```bash
EX=../../GroupDocs.Parser-for-.NET/Examples/GroupDocs.Parser.Examples.CSharp/Resources/SampleFiles
cp "$EX/sample.pdf" "$EX/images.pdf" "$EX/sample.docx" "$EX/invoice_pages.pdf" \
   "$EX/Barcodes.pdf" "$EX/samplePassword.pdf" "$EX/corrupted.png" \
   "$EX/images.xlsx" "$EX/sample.epub" "$EX/sample.htm" ./Files/
```

## Wiring

The csproj's `<None Include="..\..\Files\**\*">` glob copies these to the
test output's `Files/` subfolder. The fixture's
`SampleDocuments.ResolveSourceSampleDocs()` finds that folder at runtime
and seeds the server's storage path.

## Adding new fixtures

1. Drop a binary into this folder.
2. Add a `public const string` to `SampleDocuments.cs` referencing the filename.
3. Add the constant to the `RealSamples` array if the fixture should
   auto-load into the server's storage path for every test.
4. Write a `[Theory]` entry under the relevant `*Tests.cs` referencing the
   new constant.
5. Add a row to the table above documenting MCP method ↔ upstream example
   ↔ filename.
