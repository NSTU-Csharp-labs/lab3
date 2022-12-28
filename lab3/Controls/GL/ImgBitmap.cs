using System;
using System.Xml.Serialization;
using Avalonia;

namespace lab3.Controls.GL;

public class ImgBitmap
{
    public ImgBitmap()
    {
        Width = Height = 0;
        Pixels = Array.Empty<byte>();
    }
    public ImgBitmap(int width, int height, byte[] pixels)
    {
        Width = width;
        Height = height;
        Pixels = pixels;
    }

    public int Width { get; }

    public int Height { get; }

    public byte[] Pixels { get; }

    public AdjustedBitmap Adjust(int boundsWidth, int boundsHeight) =>
        new (this, boundsWidth, boundsHeight);
}