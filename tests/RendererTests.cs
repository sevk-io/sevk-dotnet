using Sevk.Markup;

using Xunit;

namespace Sevk.Tests;

public class RendererTests
{
    [Fact]
    public void RendersMarkupIntoAnHtmlEmailDocument()
    {
        var html = Renderer.Render("<section><paragraph>Hello world</paragraph></section>");

        Assert.StartsWith("<!DOCTYPE html PUBLIC", html);
        Assert.Contains("Hello world", html);
        Assert.DoesNotContain("<paragraph>", html);
    }
}
