namespace lab3.Controls.GL;

public struct AdjustedBitmap
{
    private readonly float _aspectRatio;
    private readonly ImgBitmap _bitmap;

    public AdjustedBitmap(ImgBitmap bitmap, int boundsWidth, int boundsHeight)
    {
        _bitmap = bitmap;

        BoundsWidth = boundsWidth;
        BoundsHeight = boundsHeight;

        AdjustedWidth = bitmap.Width;
        AdjustedHeight = bitmap.Height;
        _aspectRatio = AdjustedWidth / AdjustedHeight;

        AdjustWithWidthPriority();
        AdjustWithHeightPriority();
    }

    public int Width => _bitmap.Width;
    public int Height => _bitmap.Height;

    public int BoundsHeight { get; }
    public int BoundsWidth { get; }

    public float AdjustedWidth { get; private set; }
    public float AdjustedHeight { get; private set; }

    public byte[] Pixels => _bitmap.Pixels;

    private void AdjustWithWidthPriority()
    {
        if (AdjustedWidth <= AdjustedHeight) return;

        AdjustWidth();

        if (AdjustedHeight > BoundsHeight) AdjustHeight();
    }

    private void AdjustWithHeightPriority()
    {
        if (AdjustedWidth > AdjustedHeight) return;

        AdjustHeight();

        if (AdjustedWidth > BoundsWidth) AdjustWidth();
    }

    private void AdjustWidth()
    {
        AdjustedWidth = BoundsWidth;
        AdjustedHeight = AdjustedWidth / _aspectRatio;
    }

    private void AdjustHeight()
    {
        AdjustedHeight = BoundsHeight;
        AdjustedWidth = AdjustedHeight * _aspectRatio;
    }
}