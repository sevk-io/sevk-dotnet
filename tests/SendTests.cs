using Sevk.Types;

using Xunit;

namespace Sevk.Tests;

public class SendTests
{
    [SendFact]
    public async Task SendsAMarkupEmailThroughTheSevkApi()
    {
        var apiKey = Environment.GetEnvironmentVariable("SEVK_API_KEY");
        var from = Environment.GetEnvironmentVariable("SEVK_TEST_FROM");
        var to = Environment.GetEnvironmentVariable("SEVK_TEST_TO");
        Assert.False(string.IsNullOrEmpty(apiKey));
        Assert.False(string.IsNullOrEmpty(from));
        Assert.False(string.IsNullOrEmpty(to));

        var baseUrl = Environment.GetEnvironmentVariable("SEVK_BASE_URL");
        using var client = new SevkClient(apiKey, string.IsNullOrEmpty(baseUrl) ? null : new SevkOptions { BaseUrl = baseUrl });
        var response = await client.Emails.SendAsync(new SendEmailRequest
        {
            From = from,
            To = to,
            Subject = "Sevk SDK send test",
            Markup = "<section><paragraph>Hello from the Sevk .NET SDK</paragraph></section>"
        });

        var single = Assert.IsType<SingleEmailResponse>(response);
        Assert.NotEmpty(single.Id);
    }
}
