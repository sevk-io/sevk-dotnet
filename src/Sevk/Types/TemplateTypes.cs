namespace Sevk.Types;

public class Template
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public ContentStyle Style { get; set; } = ContentStyle.Markup;
    public string ProjectId { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";

}

public class CreateTemplateRequest
{
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public ContentStyle? Style { get; set; }
}

public class UpdateTemplateRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public ContentStyle? Style { get; set; }
}
