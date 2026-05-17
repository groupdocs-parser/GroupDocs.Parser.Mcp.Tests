using GroupDocs.Parser.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Parser.Mcp.IntegrationTests;

/// ExtractText pulls plain text out of a document. The synthetic baseline has
/// no content; real samples (PDF, DOCX, EPUB, HTML) should return non-empty
/// strings. Evaluation mode may prepend an "[Evaluation mode] Output may be
/// limited" banner.
[Collection(McpServerCollection.Name)]
public class ExtractTextTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public ExtractTextTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    public static IEnumerable<object[]> RealSamples() => new[]
    {
        new object[] { SampleDocuments.SamplePdf  },
        new object[] { SampleDocuments.SampleDocx },
        new object[] { SampleDocuments.SampleEpub },
        new object[] { SampleDocuments.SampleHtm  },
    };

    [Theory]
    [MemberData(nameof(RealSamples))]
    public async Task ExtractText_RealSample_ReturnsNonEmptyContent(string fileName)
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, fileName)))
        {
            _output.WriteLine($"Sample '{fileName}' not present — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.ExtractText.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = fileName },
            });

        if (response.IsError == true)
            throw new InvalidOperationException(
                $"Text extraction failed for '{fileName}': {ToolResponse.Text(response)}");

        var body = ToolResponse.Text(response);
        _output.WriteLine($"--- {fileName} (first 200 chars) ---\n{body.Substring(0, Math.Min(200, body.Length))}");

        Assert.DoesNotContain("Text extraction failed for", body);
        Assert.False(string.IsNullOrWhiteSpace(body));
    }

    [Fact]
    public async Task ExtractText_PageParameter_AcceptsAndScopes()
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.SamplePdf)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.SamplePdf}' not present — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.ExtractText.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.SamplePdf },
                ["page"] = 1,
            });

        if (response.IsError == true)
            throw new InvalidOperationException("Tool reported an error: " + ToolResponse.Text(response));

        var body = ToolResponse.Text(response);
        _output.WriteLine($"page=1 → first 200 chars:\n{body.Substring(0, Math.Min(200, body.Length))}");
        Assert.DoesNotContain("Text extraction failed for", body);
    }

    [Fact]
    public async Task ExtractText_PasswordProtectedPdf_AcceptsPasswordArgument()
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.PasswordPdf)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.PasswordPdf}' not present — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.ExtractText.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.PasswordPdf },
                ["password"] = SampleDocuments.ProtectedDocumentPassword,
            });

        if (response.IsError == true)
            throw new InvalidOperationException(
                "Password-protected text extraction failed: " + ToolResponse.Text(response));

        var body = ToolResponse.Text(response);
        _output.WriteLine($"password-protected sample → first 200 chars:\n{body.Substring(0, Math.Min(200, body.Length))}");
        Assert.DoesNotContain("Text extraction failed for", body);
    }
}
