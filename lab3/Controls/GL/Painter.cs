using System;
using System.Numerics;
using Avalonia.OpenGL;
using OpenTK.Graphics.OpenGL;
using static Avalonia.OpenGL.GlConsts;

namespace lab3.Controls.GL;

public class Painter : IDisposable
{
    private readonly ShaderProgram _shaderProgram;
    private readonly VertexBuffer _vertexBuffer;
    private readonly IndicesBuffer _indicesBuffer;
    private readonly Texture _texture;

    private readonly ushort[] _indices =
    {
        0, 1, 3,
        1, 2, 3
    };

    private readonly float[] _vertices =
    {
        0f, -1f, 0, -1,
        0f, 0f, 0, 0,
        1f, 0f, 1, 0,
        1f, -1f, 1, -1
    };

    private readonly Matrix4x4 _reflectionMatrix = Matrix4x4.CreateReflection
    (
        new Plane(0, 1, 0, 0)
    );

    private readonly Vector3 _cameraUp = new(0, 1, 0);

    public bool UseBlackAndWhiteFilter { get; set; }

    public bool UseRedFilter { get; set; }

    public bool UseGreenFilter { get; set; }

    public bool UseBlueFilter { get; set; }

    public Painter()
    {
        try
        {
            OpenGlUtils.CheckError();
        }
        catch
        {
            // ignored
        }

        OpenTK.Graphics.OpenGL.GL.ClearColor(0, 0, 0, 0);

        _shaderProgram = new ShaderProgram();
        var positionLocation = _shaderProgram.BindAttribLocation("aPosition");
        var texCoordLocation = _shaderProgram.BindAttribLocation("aTexCoord");
        _shaderProgram.Compile();

        _vertexBuffer = new VertexBuffer.Builder(_vertices, 4)
            .AttributeBinding(positionLocation, 2, 0)
            .AttributeBinding(texCoordLocation, 2, 2)
            .Build();

        _vertexBuffer.Use();
        _indicesBuffer = new IndicesBuffer(_indices);
        _texture = new Texture();
    }

    public void Paint(ImgBitmap bitmap, float boundsWidth, float boundsHeight)
    {
        bitmap.OnRender(boundsWidth, boundsHeight);
        _texture.SetPixels(bitmap.Pixels, bitmap.Width, bitmap.Height);

        OpenTK.Graphics.OpenGL.GL.Clear(ClearBufferMask.ColorBufferBit);
        OpenTK.Graphics.OpenGL.GL.Viewport(0, 0, (int)boundsWidth, (int)boundsHeight);

        _indicesBuffer.Use();
        _vertexBuffer.Use();
        _texture.Use();
        _shaderProgram.Use();

        SetFilters();
        SetMatrices(bitmap, boundsWidth, boundsHeight);

        OpenTK.Graphics.OpenGL.GL.DrawElements(PrimitiveType.Triangles, _indices.Length,
            DrawElementsType.UnsignedShort, 0);

        OpenGlUtils.CheckError();
    }

    public void Dispose()
    {
        _shaderProgram.Dispose();
        _vertexBuffer.Dispose();
        _indicesBuffer.Dispose();
        _texture.Dispose();
    }

    private void SetFilters()
    {
        _shaderProgram.SetUniformBool("uBlackAndWhite", UseBlackAndWhiteFilter);
        _shaderProgram.SetUniformBool("uRed", UseRedFilter);
        _shaderProgram.SetUniformBool("uGreen", UseGreenFilter);
        _shaderProgram.SetUniformBool("uBlue", UseBlueFilter);
    }

    private void SetMatrices(ImgBitmap bitmap, float boundsWidth, float boundsHeight)
    {
        _shaderProgram.SetUniformMatrix4X4("uProjection",
            GetProjectionMatrix(boundsWidth, boundsHeight));
        _shaderProgram.SetUniformMatrix4X4("uView", GetViewMatrix(bitmap));
        _shaderProgram.SetUniformMatrix4X4("uModel", GetModelMatrix(bitmap));
    }

    private Matrix4x4 GetProjectionMatrix(float boundsWidth, float boundsHeight) =>
        Matrix4x4.CreateOrthographicOffCenter
        (
            -boundsWidth / 2,
            boundsWidth / 2,
            -boundsHeight / 2,
            boundsHeight / 2,
            0,
            10
        );

    private Matrix4x4 GetViewMatrix(ImgBitmap bitmap) => Matrix4x4.CreateLookAt
    (
        new Vector3(bitmap.RenderWidth / 2, bitmap.RenderHeight / 2, 1),
        new Vector3(bitmap.RenderWidth / 2, bitmap.RenderHeight / 2, 0),
        _cameraUp
    );

    private Matrix4x4 GetModelMatrix(ImgBitmap bitmap) => Matrix4x4.Multiply
    (
        Matrix4x4.CreateScale
        (
            bitmap.RenderWidth,
            bitmap.RenderHeight,
            1
        ),
        _reflectionMatrix
    );
}