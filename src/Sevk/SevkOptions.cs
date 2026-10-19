namespace Sevk;

/// <summary>
/// Options for configuring the sevk client
/// </summary>
public class SevkOptions
{
    public string BaseUrl { get; set; } = "https://api.sevk.io";
    public int Timeout { get; set; } = 30000;
}
