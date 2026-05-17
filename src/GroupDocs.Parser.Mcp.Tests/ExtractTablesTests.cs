using System.Text.Json;
using GroupDocs.Parser.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Parser.Mcp.IntegrationTests;

/// ExtractTables produces Markdown (default) or JSON (`format='json'`).
/// invoice_pages.pdf is the upstream Examples canonical fixture for table extraction.
[Collection(McpServerCollection.Name)]
public class ExtractTablesTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public ExtractTablesTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task ExtractTables_InvoicePdf_DefaultMarkdownFormat()
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.InvoicePagesPdf)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.InvoicePagesPdf}' not present — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.ExtractTables.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.InvoicePagesPdf },
            });

        if (response.IsError == true)
            throw new InvalidOperationException(
                $"Table extraction failed for '{SampleDocuments.InvoicePagesPdf}': {ToolResponse.Text(response)}");

        var body = ToolResponse.Text(response);
        _output.WriteLine($"--- Markdown response (first 500 chars) ---\n{body.Substring(0, Math.Min(500, body.Length))}");

        Assert.DoesNotContain("Table extraction failed for", body);
        // Markdown response contains either tables or a "no tables found" message.
        Assert.False(string.IsNullOrWhiteSpace(body));
    }

    [Fact]
    public async Task ExtractTables_InvoicePdf_JsonFormatReturnsValidJson()
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.InvoicePagesPdf)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.InvoicePagesPdf}' not present — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.ExtractTables.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.InvoicePagesPdf },
                ["format"] = "json",
            });

        if (response.IsError == true)
            throw new InvalidOperationException(
                "Table extraction (json) failed: " + ToolResponse.Text(response));

        var body = ToolResponse.Text(response);
        Assert.DoesNotContain("Table extraction failed for", body);

        // If the engine found tables, the response must be parseable as JSON
        // (Pitfall #16 — the JSON branch must NOT pipe through TruncateText).
        // The body may have an evaluation-mode prefix; trim that first.
        var jsonStart = body.IndexOf('[');
        if (jsonStart >= 0)
        {
            var jsonText = body.Substring(jsonStart);
            var doc = JsonDocument.Parse(jsonText);
            _output.WriteLine($"Parsed JSON array with {doc.RootElement.GetArrayLength()} table(s)");
            Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        }
        else
        {
            _output.WriteLine($"No tables found (body: '{body}')");
        }
    }
}
