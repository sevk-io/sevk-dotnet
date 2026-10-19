<p align="center">
  <img src="https://sevk.io/logo.png" alt="sevk" width="120" />
</p>

<h1 align="center">sevk .NET SDK</h1>

<p align="center">
  Official .NET SDK for <a href="https://sevk.io">sevk</a> email platform.
</p>

<p align="center">
  <a href="https://docs.sevk.io">Documentation</a> •
  <a href="https://sevk.io">Website</a>
</p>

## Installation

Requires the .NET 6 SDK or later.

```bash
dotnet add package Sevk
```

## Send Email

```csharp
using Sevk;
using Sevk.Types;

var sevk = new SevkClient("your-api-key");

await sevk.Emails.SendAsync(new SendEmailRequest
{
    To = "recipient@example.com",
    From = "hello@yourdomain.com",
    Subject = "Hello from sevk!",
    Html = "<h1>Welcome!</h1>"
});
```

## Send Email with Markup

```csharp
using Sevk;
using Sevk.Types;
using Sevk.Markup;

var sevk = new SevkClient("your-api-key");

var html = Renderer.Render(@"
  <section padding=""40px 20px"" background-color=""#f8f9fa"">
    <container max-width=""600px"">
      <heading level=""1"" color=""#1a1a1a"">Welcome!</heading>
      <paragraph color=""#666666"">Thanks for signing up.</paragraph>
      <button href=""https://example.com"" background-color=""#5227FF"" color=""#ffffff"" padding=""12px 24px"">
        Get Started
      </button>
    </container>
  </section>
");

await sevk.Emails.SendAsync(new SendEmailRequest
{
    To = "recipient@example.com",
    From = "hello@yourdomain.com",
    Subject = "Welcome!",
    Html = html
});
```

## Documentation

For full documentation, visit [docs.sevk.io](https://docs.sevk.io)

## License

MIT
