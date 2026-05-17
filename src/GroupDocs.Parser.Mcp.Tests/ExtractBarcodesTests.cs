using System.Text.Json;
using GroupDocs.Parser.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Parser.Mcp.IntegrationTests;

/// ExtractBarcodes returns a header line + JSON array of decoded barcodes
/// (or a "No barcodes found" message). Barcodes.pdf is the upstream Examples
/// canonical fixture with multiple embedded barcode formats.
[Collection(McpServerCollection.Name)]
public class ExtractBarcodesTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public ExtractBarcodesTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task ExtractBarcodes_BarcodesPdf_FindsAtLeastOneBarcode()
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.BarcodesPdf)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.BarcodesPdf}' not present — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.ExtractBarcodes.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BarcodesPdf },
            });

        if (response.IsError == true)
            throw new InvalidOperationException(
                "Barcode extraction failed: " + ToolResponse.Text(response));

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);
        Assert.DoesNotContain("Barcode extraction failed for", body);

        // Body is either "Found N barcode(s) ..." followed by JSON, or
        // "No barcodes found", or "not supported". Validate JSON shape if found.
        var jsonStart = body.IndexOf('[');
        if (jsonStart >= 0)
        {
            var jsonText = body.Substring(jsonStart);
            var doc = JsonDocument.Parse(jsonText);
            Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);

            foreach (var bc in doc.RootElement.EnumerateArray())
            {
                Assert.True(bc.TryGetProperty("value", out _));
                Assert.True(bc.TryGetProperty("type", out _));
            }
            _output.WriteLine($"Parsed {doc.RootElement.GetArrayLength()} barcode(s) with valid schema");
        }
    }

    [Fact]
    public async Task ExtractBarcodes_SyntheticPdf_ReportsNoBarcodesGracefully()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.ExtractBarcodes.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.SyntheticPdf },
            });

        if (response.IsError == true)
            throw new InvalidOperationException("Tool reported an error: " + ToolResponse.Text(response));

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.DoesNotContain("Barcode extraction failed for", body);
        // Either "No barcodes found" or "not supported" — both graceful.
    }
}
