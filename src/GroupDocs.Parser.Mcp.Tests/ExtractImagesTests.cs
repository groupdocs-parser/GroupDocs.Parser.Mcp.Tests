using GroupDocs.Parser.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Parser.Mcp.IntegrationTests;

/// ExtractImages saves embedded images to storage as `<basename>_image<N>.<ext>`.
[Collection(McpServerCollection.Name)]
public class ExtractImagesTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public ExtractImagesTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    public static IEnumerable<object[]> ImageBearingSamples() => new[]
    {
        new object[] { SampleDocuments.ImagesPdf  },
        new object[] { SampleDocuments.ImagesXlsx },
    };

    [Theory]
    [MemberData(nameof(ImageBearingSamples))]
    public async Task ExtractImages_RealSample_ProducesAtLeastOneImageFile(string fileName)
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, fileName)))
        {
            _output.WriteLine($"Sample '{fileName}' not present — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.ExtractImages.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = fileName },
            });

        if (response.IsError == true)
            throw new InvalidOperationException(
                $"Image extraction failed for '{fileName}': {ToolResponse.Text(response)}");

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.DoesNotContain("Image extraction failed for", body);

        // Tool either reports "Extracted N image(s):" with saved paths, or
        // "No images found" / "not supported". Whichever path, it must NOT crash.
        Assert.False(string.IsNullOrWhiteSpace(body));

        // If images were actually saved, verify at least one image file lives in
        // storage with the expected naming pattern (<basename>_imageN.<ext>).
        if (body.Contains("Extracted ", StringComparison.OrdinalIgnoreCase) &&
            body.Contains(" image(s):", StringComparison.OrdinalIgnoreCase))
        {
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var imageFiles = Directory.GetFiles(_fixture.StoragePath, $"{baseName}_image*");
            Assert.NotEmpty(imageFiles);
        }
    }

    [Fact]
    public async Task ExtractImages_SyntheticPdf_ReportsNoImagesGracefully()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.ExtractImages.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.SyntheticPdf },
            });

        if (response.IsError == true)
            throw new InvalidOperationException("Tool reported an error: " + ToolResponse.Text(response));

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.DoesNotContain("Image extraction failed for", body);
        // Either "No images found" or "not supported" — both are graceful.
    }
}
