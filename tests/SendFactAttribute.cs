using Xunit;

namespace Sevk.Tests;

public sealed class SendFactAttribute : FactAttribute
{
    public SendFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SEVK_API_KEY"))
            || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SEVK_TEST_FROM"))
            || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SEVK_TEST_TO")))
        {
            Skip = "SEVK_API_KEY, SEVK_TEST_FROM and SEVK_TEST_TO are required";
        }
    }
}
