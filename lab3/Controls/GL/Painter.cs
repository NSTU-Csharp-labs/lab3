using System;
using System.Numerics;
using Avalonia.OpenGL;
using static Avalonia.OpenGL.GlConsts;

namespace lab3.Controls.GL;

public class Painter : OpenGLHelper, IDisposable
{
    private readonly Vector3 _cameraUp = new(0, 1, 0);

    private readonly ushort[] _indices =
    {
        0, 1, 3,
        1, 2, 3
    };

    private readonly IndicesBuffer _indicesBuffer;

    private readonly Matrix4x4 _reflectionMatrix = Matrix4x4.CreateReflection
    (
        new Plane(0, 1, 0, 0)
    );

    private readonly ShaderProgram _shaderProgram;
    private readonly Texture _texture;
    private readonly VertexBuffer _vertexBuffer;

    private readonly float[] _vertices =
    {
        0f, -1f, 0, -1,
        0f, 0f, 0, 0,
        1f, 0f, 1, 0,
        1f, -1f, 1, -1
    };

    public Painter(GlInterface GL, GlProfileType type) : base(GL)
    {
        GL.ClearColor(0, 0, 0, 0);

        _shaderProgram = new ShaderProgram(GL, type);
        var positionLocation = _shaderProgram.GetAttribLocation("aPosition");
        var texCoordLocation = _shaderProgram.GetAttribLocation("aTexCoord");
        _shaderProgram.Compile();

        _vertexBuffer = new VertexBuffer.Builder(GL, _vertices, 4)
            .AttributeBinding(positionLocation, 2, 0)
            .AttributeBinding(texCoordLocation, 2, 2)
            .Build();

        _vertexBuffer.Use();
        _indicesBuffer = new IndicesBuffer(GL, _indices);
        _texture = new Texture(GL);
    }

    public bool UseBlackAndWhiteFilter { get; set; }

    public bool UseRedFilter { get; set; }

    public bool UseGreenFilter { get; set; }

    public bool UseBlueFilter { get; set; }

    public void Dispose()
    {
        _shaderProgram.Dispose();
        _vertexBuffer.Dispose();
        _indicesBuffer.Dispose();
        _texture.Dispose();
    }

    public void Paint(ImgBitmap bitmap, float boundsWidth, float boundsHeight)
    {
        bitmap.OnRender(boundsWidth, boundsHeight);
        _texture.SetPixels(bitmap.Pixels, bitmap.Width, bitmap.Height);

        _gl.Clear(GL_COLOR_BUFFER_BIT);
        _gl.Viewport(0, 0, (int)boundsWidth, (int)boundsHeight);

        _indicesBuffer.Use();
        _vertexBuffer.Use();
        _texture.Use();
        _shaderProgram.Use();

        SetFilters();
        SetMatrices(bitmap, boundsWidth, boundsHeight);

        _gl.DrawElements(GL_TRIANGLES, _indices.Length, GL_UNSIGNED_SHORT, IntPtr.Zero);
        CheckError();
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
        _shaderProgram.SetUniformMatrix4X4("uProjection", GetProjectionMatrix(boundsWidth, boundsHeight));
        _shaderProgram.SetUniformMatrix4X4("uView", GetViewMatrix(bitmap));
        _shaderProgram.SetUniformMatrix4X4("uModel", GetModelMatrix(bitmap));
    }

    private Matrix4x4 GetProjectionMatrix(float boundsWidth, float boundsHeight)
    {
        return Matrix4x4.CreateOrthographicOffCenter
        (
            -boundsWidth / 2,
            boundsWidth / 2,
            -boundsHeight / 2,
            boundsHeight / 2,
            0,
            10
        );
    }

    private Matrix4x4 GetViewMatrix(ImgBitmap bitmap)
    {
        return Matrix4x4.CreateLookAt
        (
            new Vector3(bitmap.RenderWidth / 2, bitmap.RenderHeight / 2, 1),
            new Vector3(bitmap.RenderWidth / 2, bitmap.RenderHeight / 2, 0),
            _cameraUp
        );
    }

    private Matrix4x4 GetModelMatrix(ImgBitmap bitmap)
    {
        return Matrix4x4.Multiply
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
}