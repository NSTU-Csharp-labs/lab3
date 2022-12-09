using System;
using System.Xml.Serialization;
using Avalonia;

namespace lab3.Controls.GL;

public class ImgBitmap
{
    public ImgBitmap(int width, int height, byte[] pixels)
    {
        Width = width;
        Height = height;
        Pixels = pixels;

        RenderWidth = Width;
        RenderHeight = Height;
        _renderWidthToHeight = RenderWidth / RenderHeight;
    }

    public ImgBitmap() { }


    public int Width { get; }

    public int Height { get; }

    public byte[] Pixels { get; }

    [field: NonSerialized] [XmlIgnore] public float RenderWidth { get; private set; }
    
    [field: NonSerialized] [XmlIgnore] public float RenderHeight { get; private set; }

    private float _renderWidthToHeight;

    public void OnRender(float boundsWidth, float boundsHeight)
    {
        RenderWidth = Width;
        RenderHeight = Height;
        _renderWidthToHeight = RenderWidth / RenderHeight;

        RecalculateSize(boundsWidth, boundsHeight);
    }

    private void RecalculateSize(float boundsWidth, float boundsHeight)
    {
        if (RenderWidth > RenderHeight)
        {
            RecalculateImageWidth(boundsWidth);

            if (RenderHeight > boundsHeight)
            {
                RecalculateImageHeight(boundsHeight);
            }

            return;
        }

        RecalculateImageHeight(boundsHeight);

        if (RenderWidth > boundsWidth)
        {
            RecalculateImageWidth(boundsWidth);
        }
    }

    private void RecalculateImageWidth(float boundsWidth)
    {
        RenderWidth = boundsWidth;
        RenderHeight = RenderWidth / _renderWidthToHeight;
    }

    private void RecalculateImageHeight(float boundsHeight)
    {
        RenderHeight = boundsHeight;
        RenderWidth = RenderHeight * _renderWidthToHeight;
    }
}