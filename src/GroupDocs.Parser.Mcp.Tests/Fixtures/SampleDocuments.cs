using System.Text;

namespace GroupDocs.Parser.Mcp.IntegrationTests.Fixtures;

/// Real fixtures committed under the repo's `Files/` folder (sourced from the
/// upstream `GroupDocs.Parser-for-.NET/Examples/Resources/SampleFiles/` —
/// see Files/README.md for provenance) plus a synthetic minimal fixture for
/// the zero-content baseline.
internal static class SampleDocuments
{
    /// Synthetic fixture — generated at startup. Minimal 1-page PDF used as a
    /// "controlled" baseline (no text, no metadata, no images) for assertions
    /// that need a known-empty document.
    public const string SyntheticPdf = "synthetic.pdf";

    // Real samples copied from the public GroupDocs.Parser-for-.NET Examples
    // repo. See Files/README.md for method ↔ example mappings.
    public const string SamplePdf            = "sample.pdf";            // ExtractText, GetDocumentInfo
    public const string ImagesPdf            = "images.pdf";            // ExtractImages primary
    public const string ImagesXlsx           = "images.xlsx";           // ExtractImages format variant
    public const string SampleDocx           = "sample.docx";           // ExtractMetadata, GetDocumentInfo, ExtractText
    public const string InvoicePagesPdf      = "invoice_pages.pdf";     // ExtractTables
    public const string BarcodesPdf          = "Barcodes.pdf";          // ExtractBarcodes
    public const string PasswordPdf          = "samplePassword.pdf";    // password parameter tests
    public const string SampleEpub           = "sample.epub";           // ExtractText (EPUB format variant)
    public const string SampleHtm            = "sample.htm";            // ExtractText (HTML format variant)
    public const string CorruptedPng         = "corrupted.png";         // ErrorHandlingTests (intentionally corrupted)

    /// Real samples to copy into the server's storage folder at fixture startup.
    /// Tests reference these by the constants above; tests degrade gracefully
    /// via a `File.Exists(...)` guard if a sample isn't present.
    public static IReadOnlyList<string> RealSamples { get; } = new[]
    {
        SamplePdf, ImagesPdf, ImagesXlsx, SampleDocx, InvoicePagesPdf,
        BarcodesPdf, PasswordPdf, SampleEpub, SampleHtm, CorruptedPng,
    };

    /// Password for the protected PDF — sourced from the upstream Examples
    /// repo's `PasswordProtectedDocuments.cs` example to keep tests aligned
    /// with the engine team's validated fixture.
    public const string ProtectedDocumentPassword = "123456";

    public static void WriteAll(string directory)
    {
        Directory.CreateDirectory(directory);
        File.WriteAllBytes(Path.Combine(directory, SyntheticPdf), BuildBarePdf());
    }

    public static void CopyRealSamples(string targetDirectory, string? sourceDirectory)
    {
        if (string.IsNullOrEmpty(sourceDirectory) || !Directory.Exists(sourceDirectory))
            return;

        Directory.CreateDirectory(targetDirectory);
        foreach (var name in RealSamples)
        {
            var src = Path.Combine(sourceDirectory, name);
            if (File.Exists(src))
                File.Copy(src, Path.Combine(targetDirectory, name), overwrite: true);
        }
    }

    /// Resolves the source folder containing real sample files. Order:
    ///   1. GROUPDOCS_MCP_SAMPLE_DOCS env var (set by docker-compose mount).
    ///   2. `Files/` next to the test assembly — populated by the csproj
    ///      `<None Include="..\..\Files\**\*">` copy item.
    ///   3. Walk up from the assembly to find the repo's `Files/`.
    public static string? ResolveSourceSampleDocs()
    {
        var env = Environment.GetEnvironmentVariable("GROUPDOCS_MCP_SAMPLE_DOCS");
        if (!string.IsNullOrEmpty(env) && Directory.Exists(env))
            return env;

        var staged = Path.Combine(AppContext.BaseDirectory, "Files");
        if (Directory.Exists(staged))
            return staged;

        var dir = AppContext.BaseDirectory;
        for (var i = 0; i < 10 && !string.IsNullOrEmpty(dir); i++)
        {
            var candidate = Path.Combine(dir, "Files");
            if (Directory.Exists(candidate))
                return candidate;
            dir = Path.GetDirectoryName(dir);
        }

        return null;
    }

    /// Minimal PDF 1.4 with one empty page and NO content. Used as the
    /// synthetic baseline — exercises the "what happens when there's nothing
    /// to extract" path for ExtractText / ExtractImages / ExtractTables /
    /// ExtractBarcodes / ExtractMetadata.
    private static byte[] BuildBarePdf()
    {
        var body = new StringBuilder();
        var offsets = new List<int>();

        void WriteObj(string obj)
        {
            offsets.Add(body.Length);
            body.Append(obj);
        }

        body.Append("%PDF-1.4\n%\xE2\xE3\xCF\xD3\n");

        WriteObj("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
        WriteObj("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");
        WriteObj("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << >> >>\nendobj\n");
        WriteObj("4 0 obj\n<< /Title (Parser MCP synthetic baseline) /Creator (GroupDocs.Parser.Mcp integration tests) >>\nendobj\n");

        var xrefOffset = body.Length;
        body.Append("xref\n0 5\n0000000000 65535 f \n");
        foreach (var offset in offsets)
            body.Append($"{offset:D10} 00000 n \n");

        body.Append("trailer\n<< /Size 5 /Root 1 0 R /Info 4 0 R >>\n");
        body.Append($"startxref\n{xrefOffset}\n%%EOF");

        return Encoding.ASCII.GetBytes(body.ToString());
    }
}
