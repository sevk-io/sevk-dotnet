namespace Sevk.Markup;

internal sealed class RenderBudget
{
    public const int MaxMarkupDepth = 64;

    public const int MaxTagIterations = 200;

    public const int MaxRenderOutput = 5_000_000;

    public const long MaxTagScanWork = 30_000_000;

    public const int MaxBlockLength = 2500;

    public const int MaxTotalLength = 2500;

    public const string TooComplex = "Email markup is too complex to render";

    private long scanWork;

    private int highlighted;

    public void ChargeScanWork(long work)
    {
        scanWork += work;
        if (scanWork > MaxTagScanWork)
        {
            throw new ArgumentException(TooComplex);
        }
    }

    public bool Admits(int length)
    {
        return length <= MaxBlockLength && highlighted + length <= MaxTotalLength;
    }

    public void Charge(int length)
    {
        highlighted += length;
    }
}
