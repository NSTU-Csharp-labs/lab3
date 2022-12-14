using System;
using System.Collections.Generic;
using System.Numerics;
using OpenTK.Graphics.OpenGL;

namespace lab3.Controls.GL;

public class BitmapPainter : IDisposable
{
    private readonly ushort[] _indices =
    {
        0, 1, 3,
        1, 2, 3
    };

    private readonly IndicesBuffer _indicesBuffer;

    private readonly bool _isPostprocessing;
    private readonly ShaderProgram _shaderProgram;
    private readonly Texture _texture;
    private readonly VertexBuffer _vertexBuffer;

    // @formatter:off
    private readonly float[] _vertices =
    {
        // positions      texture coords
        // x    y         T  S
           1f,  1f,       1, 1, // Top Right
           1f, -1f,       1, 0, // Bottom Right
          -1f, -1f,       0, 0, // Bottom Left
          -1f,  1f,       0, 1  // Top Right
    };
    // @formatter:on

    public BitmapPainter(IEnumerable<Filter> filters, bool isPostprocessing = false)
    {
        _isPostprocessing = isPostprocessing;

        OpenTK.Graphics.OpenGL.GL.ClearColor(0, 0, 0, 0);
        // OpenTK.Graphics.OpenGL.GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

        _shaderProgram = new ShaderProgramCompiler()
            .BindAttribLocation("iPosition", out var positionLocation)
            .BindAttribLocation("iTexCoord", out var texCoordLocation)
            .AddFilters(filters)
            .Compile();

        _vertexBuffer = new VertexBuffer.Builder(_vertices, 4)
            .AttributeBinding(positionLocation, 2, 0)
            .AttributeBinding(texCoordLocation, 2, 2)
            .Build();

        _vertexBuffer.Use();
        _indicesBuffer = new IndicesBuffer(_indices);
        _texture = new Texture();
    }

    public void Dispose()
    {
        _shaderProgram.Dispose();
        _vertexBuffer.Dispose();
        _indicesBuffer.Dispose();
        _texture.Dispose();
    }

    public void Paint(AdjustedBitmap bitmap)
    {
        _texture.SetPixels(bitmap.Pixels, bitmap.Width, bitmap.Height);

        OpenTK.Graphics.OpenGL.GL.Clear(ClearBufferMask.ColorBufferBit);
        OpenTK.Graphics.OpenGL.GL.Viewport(
            0, 0,
            bitmap.BoundsWidth, bitmap.BoundsHeight
        );

        using (_indicesBuffer.Use())
        using (_vertexBuffer.Use())
        using (_texture.Use())
        using (_shaderProgram.Use())
        {
            SetScaleMatrix(bitmap);

            OpenTK.Graphics.OpenGL.GL.DrawElements(PrimitiveType.Triangles, _indices.Length,
                DrawElementsType.UnsignedShort, 0);

            OpenGlUtils.CheckError();
        }
    }

    private void SetScaleMatrix(AdjustedBitmap bitmap)
    {
        var scaleMatrix = _isPostprocessing
            ? Matrix4x4.Identity
            : Matrix4x4.CreateReflection(new Plane(0, -1, 0, 0)) *
              Matrix4x4.CreateScale(
                  bitmap.AdjustedWidth / bitmap.BoundsWidth,
                  bitmap.AdjustedHeight / bitmap.BoundsHeight, 0);

        _shaderProgram.SetUniformMatrix4X4("uScale", scaleMatrix);
    }
}