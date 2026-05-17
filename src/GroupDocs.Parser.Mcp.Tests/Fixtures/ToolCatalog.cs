using ModelContextProtocol.Client;

namespace GroupDocs.Parser.Mcp.IntegrationTests.Fixtures;

/// Resolves tool names by keyword. The server-side attribute [McpServerTool]
/// converts PascalCase method names to snake_case wire names (ExtractText →
/// extract_text, GetDocumentInfo → get_document_info). Keywords picked here are
/// snake_case substrings that uniquely identify each tool — see Pitfall #15 in
/// the clone prompt (`"documentinfo"` without the underscore does NOT match
/// `"get_document_info"`).
internal sealed class ToolCatalog
{
    private readonly IReadOnlyList<McpClientTool> _tools;

    private ToolCatalog(IReadOnlyList<McpClientTool> tools) => _tools = tools;

    public static async Task<ToolCatalog> LoadAsync(McpClient client, CancellationToken ct = default)
    {
        var tools = await client.ListToolsAsync(cancellationToken: ct);
        return new ToolCatalog(tools.ToList());
    }

    public IReadOnlyList<McpClientTool> All => _tools;

    // Each keyword is a snake_case substring of exactly one tool's wire name.
    // "text" would match extract_text only (no other tool contains "text").
    // "extract_metadata" disambiguates against "extract_text" / "extract_images" / etc.
    public McpClientTool ExtractText      => Resolve("extract_text");
    public McpClientTool ExtractImages    => Resolve("extract_images");
    public McpClientTool ExtractMetadata  => Resolve("extract_metadata");
    public McpClientTool ExtractTables    => Resolve("extract_tables");
    public McpClientTool ExtractBarcodes  => Resolve("extract_barcodes");
    public McpClientTool GetDocumentInfo  => Resolve("document_info");

    private McpClientTool Resolve(string keyword) =>
        _tools.FirstOrDefault(t => t.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                $"No tool with name containing '{keyword}'. Found: {string.Join(", ", _tools.Select(t => t.Name))}");
}
