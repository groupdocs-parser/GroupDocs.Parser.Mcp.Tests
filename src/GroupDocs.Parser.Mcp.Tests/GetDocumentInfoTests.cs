using System.Text.Json;
using GroupDocs.Parser.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Parser.Mcp.IntegrationTests;

/// GetDocumentInfo returns file type / page count / size as JSON. Should
/// work on every supported format without modifying the input.
[Collection(McpServerCollection.Name)]
public class GetDocumentInfoTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public GetDocumentInfoTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task GetDocumentInfo_SyntheticPdf_ReturnsOnePageJson()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.GetDocumentInfo.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.SyntheticPdf },
            });

        if (response.IsError == true)
            throw new InvalidOperationException("Tool reported an error: " + ToolResponse.Text(response));

        var json = ToolResponse.Json(response);
        _output.WriteLine(JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true }));

        Assert.Equal(JsonValueKind.Object, json.ValueKind);
        Assert.True(json.TryGetProperty("fileName", out var fileName));
        Assert.Equal(SampleDocuments.SyntheticPdf, fileName.GetString());

        Assert.True(json.TryGetProperty("fileType", out var fileType));
        Assert.Equal(".pdf", fileType.GetString(), ignoreCase: true);

        Assert.True(json.TryGetProperty("pageCount", out var pageCount));
        Assert.Equal(1, pageCount.GetInt32());
    }

    public static IEnumerable<object[]> RealSamples() => new[]
    {
        new object[] { SampleDocuments.SamplePdf,       ".pdf"  },
        new object[] { SampleDocuments.SampleDocx,      ".docx" },
        new object[] { SampleDocuments.ImagesXlsx,      ".xlsx" },
        new object[] { SampleDocuments.InvoicePagesPdf, ".pdf"  },
        new object[] { SampleDocuments.BarcodesPdf,     ".pdf"  },
    };

    [Theory]
    [MemberData(nameof(RealSamples))]
    public async Task GetDocumentInfo_RealSample_ReturnsValidJsonStructure(string fileName, string expectedExtension)
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, fileName)))
        {
            _output.WriteLine($"Sample '{fileName}' not present — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.GetDocumentInfo.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = fileName },
            });

        if (response.IsError == true)
            throw new InvalidOperationException(
                $"Document-info lookup failed for '{fileName}': {ToolResponse.Text(response)}");

        var json = ToolResponse.Json(response);
        _output.WriteLine(JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true }));

        Assert.True(json.TryGetProperty("fileName", out var fileNameProp));
        Assert.Equal(fileName, fileNameProp.GetString());

        Assert.True(json.TryGetProperty("fileType", out var fileType));
        Assert.Equal(expectedExtension, fileType.GetString(), ignoreCase: true);

        Assert.True(json.TryGetProperty("size", out var size));
        Assert.True(size.GetInt64() > 0);

        // pageCount may be null for some formats — just ensure the property exists.
        Assert.True(json.TryGetProperty("pageCount", out _));
    }
}
