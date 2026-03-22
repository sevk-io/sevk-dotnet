using Sevk.Markup;
using Xunit;

namespace Sevk.Tests;

public class MarkupTests
{
    // Helper: extract body content from full HTML
    private static string RenderBody(string markup)
    {
        var html = Renderer.Render(markup);
        var match = System.Text.RegularExpressions.Regex.Match(html, @"<body[^>]*>([\s\S]*)</body>");
        return match.Success ? match.Groups[1].Value.Trim() : html;
    }

    // ============================================
    // EXISTING TESTS
    // ============================================

    [Fact]
    public void ShouldRenderSection()
    {
        var markup = "<sevk-section background-color=\"#f5f5f5\">Content</sevk-section>";
        var html = Renderer.Render(markup);
        Assert.Contains("<table", html);
        Assert.Contains("background-color:#f5f5f5", html);
    }

    [Fact]
    public void ShouldRenderContainer()
    {
        var markup = "<sevk-container max-width=\"600px\">Content</sevk-container>";
        var html = Renderer.Render(markup);
        Assert.Contains("max-width:600px", html);
    }

    [Fact]
    public void ShouldRenderHeading()
    {
        var markup = "<sevk-heading level=\"2\" color=\"#333\">Title</sevk-heading>";
        var html = Renderer.Render(markup);
        Assert.Contains("<h2", html);
        Assert.Contains("color:#333", html);
    }

    [Fact]
    public void ShouldRenderButton()
    {
        var markup = "<sevk-button href=\"https://example.com\" background-color=\"#007bff\">Click</sevk-button>";
        var html = Renderer.Render(markup);
        Assert.Contains("href=\"https://example.com\"", html);
        Assert.Contains("background-color:#007bff", html);
    }

    [Fact]
    public void ShouldRenderImage()
    {
        var markup = "<sevk-image src=\"https://example.com/img.png\" alt=\"Test\" width=\"200\"></sevk-image>";
        var html = Renderer.Render(markup);
        Assert.Contains("<img", html);
        Assert.Contains("src=\"https://example.com/img.png\"", html);
        Assert.Contains("alt=\"Test\"", html);
    }

    [Fact]
    public void ShouldRenderEmptyMarkup()
    {
        var html = Renderer.Render("");
        Assert.Equal("", html);
    }

    [Fact]
    public void ShouldRenderDivider()
    {
        var markup = "<sevk-divider color=\"#ccc\" thickness=\"2px\"></sevk-divider>";
        var html = Renderer.Render(markup);
        Assert.Contains("<div", html);
        Assert.Contains("#ccc", html);
    }

    [Fact]
    public void ShouldRenderLink()
    {
        var markup = "<sevk-link href=\"https://example.com\" color=\"#007bff\">Click here</sevk-link>";
        var html = Renderer.Render(markup);
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
        var html = Renderer.Render(markup);
        Assert.Contains("<table", html);
        Assert.Contains("<h1", html);
        Assert.Contains("Get Started", html);
    }

    [Fact]
    public void ShouldPreserveRegularHtml()
    {
        var markup = "<p>Regular paragraph</p><sevk-button href=\"#\">Click</sevk-button>";
        var html = Renderer.Render(markup);
        Assert.Contains("<p>Regular paragraph</p>", html);
        Assert.Contains("Click", html);
    }

    // ============================================
    // BLOCK TEMPLATE ENGINE - Simple Variables
    // ============================================

    [Fact]
    public void Block_InjectsSimpleVariable()
    {
        var html = Renderer.Render("<block config=\"{'name':'Sevk'}\"><paragraph>{%name%}</paragraph></block>");
        Assert.Contains("Sevk", html);
        Assert.Contains("<p", html);
    }

    [Fact]
    public void Block_InjectsMultipleVariables()
    {
        var html = Renderer.Render("<block config=\"{'a':'X','b':'Y'}\"><paragraph>{%a%}-{%b%}</paragraph></block>");
        Assert.Contains("X-Y", html);
    }

    [Fact]
    public void Block_ReturnsEmptyForMissingVariable()
    {
        var html = Renderer.Render("<block config=\"{}\"><paragraph>{%missing%}</paragraph></block>");
        Assert.DoesNotContain("{%missing%}", html);
    }

    // ============================================
    // BLOCK TEMPLATE ENGINE - Fallback Values
    // ============================================

    [Fact]
    public void Block_UsesValueOverFallback()
    {
        var html = Renderer.Render("<block config=\"{'color':'#fff'}\"><paragraph color=\"{%color ?? #000%}\">text</paragraph></block>");
        Assert.Contains("#fff", html);
    }

    [Fact]
    public void Block_UsesFallbackWhenMissing()
    {
        var html = Renderer.Render("<block config=\"{}\"><paragraph color=\"{%color ?? #000%}\">text</paragraph></block>");
        Assert.Contains("#000", html);
    }

    [Fact]
    public void Block_FallbackWithEmptyConfig()
    {
        var html = Renderer.Render("<block config=\"{}\"><section padding=\"{%padding ?? 20px%}\" background-color=\"{%bg ?? #fff%}\"><paragraph color=\"{%color ?? #333%}\">text</paragraph></section></block>");
        Assert.Contains("20px", html);
        Assert.Contains("#fff", html);
        Assert.Contains("#333", html);
    }

    // ============================================
    // BLOCK TEMPLATE ENGINE - Each Loop
    // ============================================

    [Fact]
    public void Block_EachLoopProducesMultipleElements()
    {
        var config = "{'items':[{'name':'One'},{'name':'Two'},{'name':'Three'}]}";
        var html = Renderer.Render($"<block config=\"{config}\">{{%#each items as item%}}<paragraph>{{%item.name%}}</paragraph>{{%/each%}}</block>");
        Assert.Contains("One", html);
        Assert.Contains("Two", html);
        Assert.Contains("Three", html);
    }

    [Fact]
    public void Block_EachLoopWithEmptyArrayProducesNothing()
    {
        var html = Renderer.Render("<block config=\"{'items':[]}\"><paragraph>{%#each items as i%}{%i.x%}{%/each%}</paragraph></block>");
        Assert.DoesNotContain("{%", html);
    }

    [Fact]
    public void Block_EachLoopWithMultipleProperties()
    {
        var config = "{'links':[{'href':'/about','label':'About'},{'href':'/contact','label':'Contact'}]}";
        var html = Renderer.Render($"<block config=\"{config}\">{{%#each links as link%}}<sevk-link href=\"{{%link.href%}}\">{{%link.label%}}</sevk-link>{{%/each%}}</block>");
        Assert.Contains("About", html);
        Assert.Contains("Contact", html);
        Assert.Contains("/about", html);
        Assert.Contains("/contact", html);
    }

    // ============================================
    // BLOCK TEMPLATE ENGINE - If/Else
    // ============================================

    [Fact]
    public void Block_IfConditionalHidesContent()
    {
        var html = Renderer.Render("<block config=\"{'show':false}\">{%#if show%}<paragraph>hidden</paragraph>{%/if%}<paragraph>visible</paragraph></block>");
        Assert.DoesNotContain("hidden", html);
        Assert.Contains("visible", html);
    }

    [Fact]
    public void Block_IfElseShowsCorrectBranch()
    {
        var html = Renderer.Render("<block config=\"{'mode':'dark'}\">{%#if mode%}<section background-color=\"#000\"><paragraph color=\"#fff\">Dark</paragraph></section>{%else%}<section><paragraph>Light</paragraph></section>{%/if%}</block>");
        Assert.Contains("#000", html);
        Assert.Contains("Dark", html);
        Assert.DoesNotContain("Light", html);
    }

    [Fact]
    public void Block_IfFalseShowsElseBranch()
    {
        var html = Renderer.Render("<block config=\"{'show':false}\">{%#if show%}<paragraph>YES</paragraph>{%else%}<paragraph>NO</paragraph>{%/if%}</block>");
        Assert.DoesNotContain("YES", html);
        Assert.Contains("NO", html);
    }

    // ============================================
    // BLOCK TEMPLATE ENGINE - Nested If
    // ============================================

    [Fact]
    public void Block_NestedIfBothTrue()
    {
        var html = Renderer.Render("<block config=\"{'a':true,'b':true}\">{%#if a%}{%#if b%}<paragraph>AB</paragraph>{%else%}<paragraph>A</paragraph>{%/if%}{%else%}<paragraph>NONE</paragraph>{%/if%}</block>");
        Assert.Contains("AB", html);
        Assert.DoesNotContain("NONE", html);
    }

    [Fact]
    public void Block_NestedIfOuterTrueInnerFalse()
    {
        var html = Renderer.Render("<block config=\"{'a':true,'b':false}\">{%#if a%}{%#if b%}<paragraph>AB</paragraph>{%else%}<paragraph>A_ONLY</paragraph>{%/if%}{%else%}<paragraph>NONE</paragraph>{%/if%}</block>");
        Assert.Contains("A_ONLY", html);
        Assert.DoesNotContain("NONE", html);
    }

    [Fact]
    public void Block_NestedIfOuterFalse()
    {
        var html = Renderer.Render("<block config=\"{'a':false,'b':true}\">{%#if a%}{%#if b%}<paragraph>AB</paragraph>{%else%}<paragraph>A</paragraph>{%/if%}{%else%}<paragraph>NONE</paragraph>{%/if%}</block>");
        Assert.Contains("NONE", html);
    }

    // ============================================
    // BLOCK TEMPLATE ENGINE - {{variable}} Preservation
    // ============================================

    [Fact]
    public void Block_PreservesDoubleBraceVariables()
    {
        var html = Renderer.Render("<block config=\"{'text':'Unsub'}\"><paragraph><sevk-link href=\"{{unsubscribeUrl}}\">{%text%}</sevk-link></paragraph></block>");
        Assert.Contains("{{unsubscribeUrl}}", html);
        Assert.Contains("Unsub", html);
    }

    [Fact]
    public void Block_PreservesUnsubscribeUrlInFullPipeline()
    {
        var config = "{'text':'You subscribed.','linkText':'Unsub','textColor':'#999','linkColor':'#999','backgroundColor':'#f8f9fa'}";
        var template = "<section background-color=\"{%backgroundColor ?? #f8f9fa%}\"><paragraph color=\"{%textColor%}\">{%text%}</paragraph><paragraph><sevk-link href=\"{{unsubscribeUrl}}\" color=\"{%linkColor%}\">{%linkText%}</sevk-link></paragraph></section>";
        var html = Renderer.Render($"<block config=\"{config}\">{template}</block>");
        Assert.Contains("You subscribed.", html);
        Assert.Contains("Unsub", html);
        Assert.Contains("{{unsubscribeUrl}}", html);
        Assert.Contains("#f8f9fa", html);
    }

    // ============================================
    // BLOCK WITH MARKUP ELEMENTS
    // ============================================

    [Fact]
    public void Block_WithParagraphProducesHtml()
    {
        var html = Renderer.Render("<block config=\"{'text':'Hello'}\"><paragraph>{%text%}</paragraph></block>");
        Assert.Contains("Hello", html);
        Assert.Contains("<p", html);
    }

    [Fact]
    public void Block_WithHeadingRendersCorrectly()
    {
        var html = Renderer.Render("<block config=\"{'title':'Welcome'}\"><heading level=\"1\">{%title%}</heading></block>");
        Assert.Contains("Welcome", html);
        Assert.Contains("<h1", html);
    }

    [Fact]
    public void Block_WithButtonRendersLink()
    {
        var html = Renderer.Render("<block config=\"{'url':'https://sevk.io','label':'Click'}\"><button href=\"{%url%}\">{%label%}</button></block>");
        Assert.Contains("https://sevk.io", html);
        Assert.Contains("Click", html);
    }

    [Fact]
    public void Block_WithImageRendersImgTag()
    {
        var html = Renderer.Render("<block config=\"{'src':'logo.png','w':'100'}\"><image src=\"{%src%}\" width=\"{%w%}px\"></image></block>");
        Assert.Contains("logo.png", html);
        Assert.Contains("100", html);
    }

    [Fact]
    public void Block_WithSectionAndBackgroundColor()
    {
        var html = Renderer.Render("<block config=\"{'bg':'#f0f0f0'}\"><section background-color=\"{%bg%}\"><paragraph>test</paragraph></section></block>");
        Assert.Contains("#f0f0f0", html);
        Assert.Contains("test", html);
    }

    [Fact]
    public void Block_WithLinkRendersAnchor()
    {
        var html = Renderer.Render("<block config=\"{'href':'https://sevk.io','text':'Visit'}\"><paragraph><sevk-link href=\"{%href%}\">{%text%}</sevk-link></paragraph></block>");
        Assert.Contains("href=\"https://sevk.io\"", html);
        Assert.Contains("Visit", html);
    }

    // ============================================
    // MULTIPLE BLOCKS AND COMPLEX TEMPLATES
    // ============================================

    [Fact]
    public void Block_MultipleBlocksInSameDocument()
    {
        var html = Renderer.Render(@"
            <block config=""{'title':'Header'}""><heading level=""1"">{%title%}</heading></block>
            <paragraph>Content between blocks</paragraph>
            <block config=""{'footer':'Footer text'}""><paragraph>{%footer%}</paragraph></block>
        ");
        Assert.Contains("Header", html);
        Assert.Contains("Content between blocks", html);
        Assert.Contains("Footer text", html);
    }

    [Fact]
    public void Block_SocialLinksTemplateEndToEnd()
    {
        var config = "{'title':'Follow us','titleColor':'#666','links':[{'href':'https://twitter.com','iconSrc':'https://cdn.sevk.io/icons/x-twitter.png','platform':'x-twitter'}],'iconSize':32,'alignment':'center'}";
        var template = "<section text-align=\"{%alignment ?? center%}\">{%#if title%}<paragraph color=\"{%titleColor ?? #666%}\">{%title%}</paragraph>{%/if%}{%#each links as link%}<sevk-link href=\"{%link.href%}\"><image src=\"{%link.iconSrc%}\" width=\"{%iconSize%}px\" alt=\"{%link.platform%}\"></image></sevk-link>{%/each%}</section>";
        var html = Renderer.Render($"<block config=\"{config}\">{template}</block>");
        Assert.Contains("Follow us", html);
        Assert.Contains("https://twitter.com", html);
        Assert.Contains("x-twitter.png", html);
        Assert.Contains("32", html);
    }

    [Fact]
    public void Block_HeaderTemplateEndToEndCentered()
    {
        var config = "{'centered':true,'title':'Brand','titleColor':'#1a1a1a','links':[{'href':'/about','label':'About'}],'linkColor':'#666'}";
        var template = "{%#if centered%}<section text-align=\"center\">{%#if title%}<heading level=\"3\" color=\"{%titleColor%}\">{%title%}</heading>{%/if%}{%#if links%}<section>{%#each links as link%}<sevk-link href=\"{%link.href%}\" color=\"{%linkColor%}\">{%link.label%}</sevk-link>{%/each%}</section>{%/if%}</section>{%else%}<section><paragraph>side</paragraph></section>{%/if%}";
        var html = Renderer.Render($"<block config=\"{config}\">{template}</block>");
        Assert.Contains("Brand", html);
        Assert.Contains("About", html);
        Assert.Contains("center", html);
        Assert.DoesNotContain("side", html);
    }

    // ============================================
    // BLOCK WITH ROW/COLUMN LAYOUT
    // ============================================

    [Fact]
    public void Block_WithRowColumnLayout()
    {
        var html = Renderer.Render("<block config=\"{'left':'L','right':'R'}\"><row><column><paragraph>{%left%}</paragraph></column><column><paragraph>{%right%}</paragraph></column></row></block>");
        Assert.Contains("L", html);
        Assert.Contains("R", html);
    }

    // ============================================
    // BLOCK - CONFIG EDGE CASES
    // ============================================

    [Fact]
    public void Block_HandlesEmptyConfig()
    {
        var html = Renderer.Render("<block config=\"{}\"><paragraph>{%x ?? fallback%}</paragraph></block>");
        Assert.Contains("fallback", html);
    }

    [Fact]
    public void Block_ReturnsEmptyForMissingTemplate()
    {
        var html = Renderer.Render("<block config=\"{}\"></block>");
        // Block with no template should produce nothing from the block itself
        Assert.DoesNotContain("{%", html);
    }

    [Fact]
    public void Block_HandlesHtmlEntitiesInConfig()
    {
        var html = Renderer.Render("<block config=\"{&quot;name&quot;:&quot;test&quot;}\"><paragraph>{%name%}</paragraph></block>");
        Assert.Contains("test", html);
    }

    [Fact]
    public void Block_HandlesAmpersandEntitiesInConfig()
    {
        var html = Renderer.Render("<block config=\"{&quot;x&quot;:&quot;a&amp;b&quot;}\"><paragraph>{%x%}</paragraph></block>");
        Assert.Contains("a&b", html);
    }

    [Fact]
    public void Block_HandlesInvalidJsonGracefully()
    {
        var html = Renderer.Render("<block config=\"not json\"><paragraph>{%x ?? ok%}</paragraph></block>");
        Assert.Contains("ok", html);
    }

    // ============================================
    // BLOCK - COMBINED TEMPLATE FEATURES
    // ============================================

    [Fact]
    public void Block_IfPlusEachPlusVariables()
    {
        var config = "{'title':'Nav','links':[{'href':'/a','label':'A'},{'href':'/b','label':'B'}]}";
        var template = "{%#if title%}<heading level=\"1\">{%title%}</heading>{%/if%}{%#each links as l%}<sevk-link href=\"{%l.href%}\">{%l.label%}</sevk-link>{%/each%}";
        var html = Renderer.Render($"<block config=\"{config}\">{template}</block>");
        Assert.Contains("Nav", html);
        Assert.Contains("A", html);
        Assert.Contains("B", html);
    }

    [Fact]
    public void Block_IfFalseHidesEntireSectionIncludingEach()
    {
        var html = Renderer.Render("<block config=\"{'show':false,'items':[{'x':'A'}]}\">{%#if show%}{%#each items as i%}<paragraph>{%i.x%}</paragraph>{%/each%}{%/if%}</block>");
        Assert.DoesNotContain(">A<", html);
    }

    // ============================================
    // DOCUMENT STRUCTURE
    // ============================================

    [Fact]
    public void Document_ProducesValidHtmlDocument()
    {
        var html = Renderer.Render("<paragraph>Hello</paragraph>");
        Assert.Contains("<!DOCTYPE html", html);
        Assert.Contains("<html", html);
        Assert.Contains("<head>", html);
        Assert.Contains("<body", html);
        Assert.Contains("</html>", html);
    }

    [Fact]
    public void Document_IncludesCharsetMeta()
    {
        var html = Renderer.Render("<paragraph>test</paragraph>");
        Assert.Contains("charset=UTF-8", html);
    }

    [Fact]
    public void Document_IncludesViewportMeta()
    {
        var html = Renderer.Render("<paragraph>test</paragraph>");
        Assert.Contains("viewport", html);
    }

    [Fact]
    public void Document_IncludesEmailSafeBaseStyles()
    {
        var html = Renderer.Render("<paragraph>test</paragraph>");
        Assert.Contains("border-collapse", html);
    }

    [Fact]
    public void Document_IncludesMsoConditionalComments()
    {
        var html = Renderer.Render("<paragraph>test</paragraph>");
        Assert.Contains("<!--[if mso]>", html);
    }

    [Fact]
    public void Document_WrapsContentInRoleArticleDiv()
    {
        var body = RenderBody("<paragraph>test</paragraph>");
        Assert.Contains("role=\"article\"", body);
    }

    // ============================================
    // PARAGRAPH
    // ============================================

    [Fact]
    public void Paragraph_RendersBasicParagraph()
    {
        var body = RenderBody("<paragraph>Hello world</paragraph>");
        Assert.Contains("Hello world", body);
        Assert.Contains("<p", body);
    }

    [Fact]
    public void Paragraph_WithColor()
    {
        var body = RenderBody("<paragraph color=\"#ff0000\">Red</paragraph>");
        Assert.Contains("#ff0000", body);
        Assert.Contains("Red", body);
    }

    [Fact]
    public void Paragraph_WithFontSize()
    {
        var body = RenderBody("<paragraph font-size=\"18px\">Big</paragraph>");
        Assert.Contains("18px", body);
    }

    [Fact]
    public void Paragraph_WithTextAlign()
    {
        var body = RenderBody("<paragraph text-align=\"center\">Centered</paragraph>");
        Assert.Contains("text-align:center", body);
    }

    [Fact]
    public void Paragraph_WithPadding()
    {
        var body = RenderBody("<paragraph padding=\"10px 20px\">Padded</paragraph>");
        Assert.Contains("10px 20px", body);
    }

    [Fact]
    public void Paragraph_WithMultipleAttributes()
    {
        var body = RenderBody("<paragraph color=\"#333\" font-size=\"14px\" text-align=\"left\">Multi</paragraph>");
        Assert.Contains("#333", body);
        Assert.Contains("14px", body);
        Assert.Contains("Multi", body);
    }

    // ============================================
    // HEADING
    // ============================================

    [Fact]
    public void Heading_RendersH1()
    {
        var body = RenderBody("<heading level=\"1\">Title</heading>");
        Assert.Contains("<h1", body);
        Assert.Contains("Title", body);
    }

    [Fact]
    public void Heading_RendersH2()
    {
        var body = RenderBody("<heading level=\"2\">Subtitle</heading>");
        Assert.Contains("<h2", body);
    }

    [Fact]
    public void Heading_WithColor()
    {
        var body = RenderBody("<heading level=\"1\" color=\"#1a1a1a\">Colored</heading>");
        Assert.Contains("color:#1a1a1a", body);
    }

    [Fact]
    public void Heading_WithFontSize()
    {
        var body = RenderBody("<heading level=\"1\" font-size=\"32px\">Large</heading>");
        Assert.Contains("32px", body);
    }

    // ============================================
    // BUTTON
    // ============================================

    [Fact]
    public void Button_RendersWithHref()
    {
        var body = RenderBody("<button href=\"https://example.com\">Click me</button>");
        Assert.Contains("href=\"https://example.com\"", body);
        Assert.Contains("Click me", body);
    }

    [Fact]
    public void Button_WithBackgroundColor()
    {
        var body = RenderBody("<button href=\"#\" background-color=\"#007bff\">Action</button>");
        Assert.Contains("background-color:#007bff", body);
    }

    [Fact]
    public void Button_WithPadding()
    {
        var body = RenderBody("<button href=\"#\" padding=\"12px 24px\">Padded</button>");
        Assert.Contains("Padded", body);
    }

    [Fact]
    public void Button_IncludesMsoCompatibility()
    {
        var body = RenderBody("<button href=\"#\">MSO</button>");
        Assert.Contains("<!--[if mso]>", body);
    }

    // ============================================
    // SECTION
    // ============================================

    [Fact]
    public void Section_RendersAsTable()
    {
        var body = RenderBody("<section>Inner</section>");
        Assert.Contains("<table", body);
        Assert.Contains("Inner", body);
    }

    [Fact]
    public void Section_WithBackgroundColor()
    {
        var body = RenderBody("<section background-color=\"#f5f5f5\">Content</section>");
        Assert.Contains("background-color:#f5f5f5", body);
    }

    [Fact]
    public void Section_WithTextAlign()
    {
        var body = RenderBody("<section text-align=\"center\">Centered</section>");
        Assert.Contains("text-align:center", body);
    }

    // ============================================
    // CONTAINER
    // ============================================

    [Fact]
    public void Container_RendersAsTable()
    {
        var body = RenderBody("<container max-width=\"600px\">Content</container>");
        Assert.Contains("<table", body);
        Assert.Contains("max-width:600px", body);
    }

    [Fact]
    public void Container_WithPadding()
    {
        var body = RenderBody("<container padding=\"20px\">Padded</container>");
        Assert.Contains("padding:20px", body);
    }

    [Fact]
    public void Container_WithBorderRadius()
    {
        var body = RenderBody("<container border-radius=\"8px\">Rounded</container>");
        Assert.Contains("border-radius:8px", body);
        Assert.Contains("border-collapse:separate", body);
    }

    // ============================================
    // ROW AND COLUMN
    // ============================================

    [Fact]
    public void Row_RendersAsTable()
    {
        var body = RenderBody("<row><column>Col</column></row>");
        Assert.Contains("<table", body);
        Assert.Contains("Col", body);
    }

    [Fact]
    public void Row_WithMultipleColumns()
    {
        var body = RenderBody("<row><column>Left</column><column>Right</column></row>");
        Assert.Contains("Left", body);
        Assert.Contains("Right", body);
        Assert.Contains("sevk-column", body);
    }

    [Fact]
    public void Row_WithGap()
    {
        var body = RenderBody("<row gap=\"16px\"><column>A</column><column>B</column></row>");
        Assert.Contains("A", body);
        Assert.Contains("B", body);
    }

    [Fact]
    public void Column_HasVerticalAlignTop()
    {
        var body = RenderBody("<row><column>C</column></row>");
        Assert.Contains("vertical-align:top", body);
    }

    // ============================================
    // LANG AND DIR SUPPORT
    // ============================================

    [Fact]
    public void LangDir_DefaultsToEnLtr()
    {
        var html = Renderer.Render("<paragraph>test</paragraph>");
        Assert.Contains("lang=\"en\"", html);
        Assert.Contains("dir=\"ltr\"", html);
    }

    [Fact]
    public void LangDir_ParsesLangFromMailTag()
    {
        var html = Renderer.Render("<mail lang=\"fr\"><body><paragraph>Bonjour</paragraph></body></mail>");
        Assert.Contains("lang=\"fr\"", html);
        Assert.Contains("dir=\"ltr\"", html);
    }

    [Fact]
    public void LangDir_ParsesDirFromMailTag()
    {
        var html = Renderer.Render("<mail dir=\"rtl\"><body><paragraph>مرحبا</paragraph></body></mail>");
        Assert.Contains("lang=\"en\"", html);
        Assert.Contains("dir=\"rtl\"", html);
    }

    [Fact]
    public void LangDir_ParsesBothFromMailTag()
    {
        var html = Renderer.Render("<mail lang=\"ar\" dir=\"rtl\"><body><paragraph>مرحبا</paragraph></body></mail>");
        Assert.Contains("lang=\"ar\"", html);
        Assert.Contains("dir=\"rtl\"", html);
    }

    [Fact]
    public void LangDir_ParsesFromEmailTag()
    {
        var html = Renderer.Render("<email lang=\"de\" dir=\"ltr\"><body><paragraph>Hallo</paragraph></body></email>");
        Assert.Contains("lang=\"de\"", html);
        Assert.Contains("dir=\"ltr\"", html);
    }

    [Fact]
    public void LangDir_HeadSettingsOverride()
    {
        var headSettings = new EmailHeadSettings { Lang = "ja", Dir = "ltr" };
        var html = Renderer.Render("<paragraph>test</paragraph>", headSettings);
        Assert.Contains("lang=\"ja\"", html);
        Assert.Contains("dir=\"ltr\"", html);
    }
}
