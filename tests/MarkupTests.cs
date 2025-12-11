using Sevk.Markup;
using Xunit;

namespace Sevk.Tests;

public class MarkupTests
{
    [Fact]
    public void ShouldRenderSection()
    {
        var markup = "<sevk-section background-color=\"#f5f5f5\">Content</sevk-section>";
        var html = MarkupRenderer.Render(markup);
        Assert.Contains("<table", html);
        Assert.Contains("background-color:#f5f5f5", html);
    }

    [Fact]
    public void ShouldRenderContainer()
    {
        var markup = "<sevk-container max-width=\"600px\">Content</sevk-container>";
        var html = MarkupRenderer.Render(markup);
        Assert.Contains("max-width:600px", html);
    }

    [Fact]
    public void ShouldRenderHeading()
    {
        var markup = "<sevk-heading level=\"2\" color=\"#333\">Title</sevk-heading>";
        var html = MarkupRenderer.Render(markup);
        Assert.Contains("<h2", html);
        Assert.Contains("color:#333", html);
    }

    [Fact]
    public void ShouldRenderButton()
    {
        var markup = "<sevk-button href=\"https://example.com\" background-color=\"#007bff\">Click</sevk-button>";
        var html = MarkupRenderer.Render(markup);
        Assert.Contains("href=\"https://example.com\"", html);
        Assert.Contains("background-color:#007bff", html);
    }

    [Fact]
    public void ShouldRenderImage()
    {
        var markup = "<sevk-image src=\"https://example.com/img.png\" alt=\"Test\" width=\"200\"></sevk-image>";
        var html = MarkupRenderer.Render(markup);
        Assert.Contains("<img", html);
        Assert.Contains("src=\"https://example.com/img.png\"", html);
        Assert.Contains("alt=\"Test\"", html);
    }

    [Fact]
    public void ShouldRenderEmptyMarkup()
    {
        var html = MarkupRenderer.Render("");
        Assert.Equal("", html);
    }

    [Fact]
    public void ShouldRenderDivider()
    {
        var markup = "<sevk-divider color=\"#ccc\" thickness=\"2px\"></sevk-divider>";
        var html = MarkupRenderer.Render(markup);
        Assert.Contains("<div", html);
        Assert.Contains("background-color", html);
    }

    [Fact]
    public void ShouldRenderLink()
    {
        var markup = "<sevk-link href=\"https://example.com\" color=\"#007bff\">Click here</sevk-link>";
        var html = MarkupRenderer.Render(markup);
        Assert.Contains("<a", html);
        Assert.Contains("href=\"https://example.com\"", html);
    }

    [Fact]
    public void ShouldRenderNestedComponents()
    {
        var markup = @"
<sevk-section background-color=""#ffffff"">
    <sevk-container max-width=""600px"">
        <sevk-heading level=""1"">Welcome</sevk-heading>
        <sevk-button href=""https://example.com"">Get Started</sevk-button>
    </sevk-container>
</sevk-section>";
        var html = MarkupRenderer.Render(markup);
        Assert.Contains("<table", html);
        Assert.Contains("<h1", html);
        Assert.Contains("Get Started", html);
    }

    [Fact]
    public void ShouldPreserveRegularHtml()
    {
        var markup = "<p>Regular paragraph</p><sevk-button href=\"#\">Click</sevk-button>";
        var html = MarkupRenderer.Render(markup);
        Assert.Contains("<p>Regular paragraph</p>", html);
        Assert.Contains("Click", html);
    }
}
