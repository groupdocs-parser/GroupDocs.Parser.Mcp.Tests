using System.Text.Json;
using GroupDocs.Parser.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Parser.Mcp.IntegrationTests;

/// ExtractMetadata returns a JSON object of metadata key-value pairs.
[Collection(McpServerCollection.Name)]
public class ExtractMetadataTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public ExtractMetadataTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task ExtractMetadata_SampleDocx_ReturnsValidJson()
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.SampleDocx)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.SampleDocx}' not present — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.ExtractMetadata.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.SampleDocx },
            });

        if (response.IsError == true)
            throw new InvalidOperationException(
                "Metadata extraction failed: " + ToolResponse.Text(response));

        var body = ToolResponse.Text(response);
        Assert.DoesNotContain("Metadata extraction failed for", body);

        // Must be a valid JSON object (Pitfall #16 — not piped through TruncateText).
        if (!body.StartsWith("No metadata", StringComparison.OrdinalIgnoreCase))
        {
            var json = ToolResponse.Json(response);
            _output.WriteLine(JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true }));
            Assert.Equal(JsonValueKind.Object, json.ValueKind);
        }
    }

    [Fact]
    public async Task ExtractMetadata_SamplePdf_ReturnsValidJsonOrGracefulMessage()
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.SamplePdf)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.SamplePdf}' not present — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.ExtractMetadata.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.SamplePdf },
            });

        if (response.IsError == true)
            throw new InvalidOperationException(
                "Metadata extraction failed: " + ToolResponse.Text(response));

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);
        Assert.DoesNotContain("Metadata extraction failed for", body);

        // PDF may or may not have metadata — accept either valid JSON or the
        // "No metadata found" message.
        if (!body.StartsWith("No metadata", StringComparison.OrdinalIgnoreCase))
        {
            var json = ToolResponse.Json(response);
            Assert.Equal(JsonValueKind.Object, json.ValueKind);
        }
    }
}
