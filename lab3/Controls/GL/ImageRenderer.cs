using System;
using System.Numerics;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static Avalonia.OpenGL.GlConsts;

namespace lab3.Controls.GL;

public class ImageRenderer : OpenGlControlBase
{
    private readonly Vector3 _cameraUp = new(0, 1, 0);

    private readonly ushort[] _indices =
    {
        0, 1, 3,
        1, 2, 3
    };

    private readonly Matrix4x4 _reflectionMatrix = Matrix4x4.CreateReflection
    (
        new Plane(0, 1, 0, 0)
    );

    private readonly float[] _vertices =
    {
        1f, 1f, 0, -1,
        1f, -1f, 0, 0,
        -1f, -1f, 1, 0,
        -1f, 1f, 1, -1
    };

    private float _imageHeight;

    private float _imageWidth;

    private IndicesBuffer _indicesBuffer;
    private ShaderProgram _shaderProgram;
    private Texture _texture;
    private VertexBuffer _vertexBuffer;

    private float _widthToHeight;

    private Matrix4x4 _projectionMatrix => Matrix4x4.CreateOrthographicOffCenter
    (
        -(float)Bounds.Width / 2,
        (float)Bounds.Width / 2,
        -(float)Bounds.Height / 2,
        (float)Bounds.Height / 2,
        0,
        10
    );

    private Matrix4x4 _viewMatrix => Matrix4x4.CreateLookAt
    (
        new Vector3(_imageWidth / 2, _imageHeight / 2, 1),
        new Vector3(_imageWidth / 2, _imageHeight / 2, 0),
        _cameraUp
    );

    private Matrix4x4 _modelMatrix => Matrix4x4.Multiply
    (
        Matrix4x4.CreateScale
        (
            _imageWidth,
            _imageHeight,
            1
        ),
        _reflectionMatrix
    );

    private string FragmentShaderSource => @"
    varying vec2 vTexCoord;
    uniform sampler2D uTexture;

    void main()
    {
        gl_FragColor = texture(uTexture, vTexCoord);
    }
    ";

    private string VertexShaderSource => @"
    attribute vec2 aPosition;
    attribute vec2 aTexCoord;
    uniform mat4 uProjection;
    uniform mat4 uView;
    uniform mat4 uModel;

    varying vec2 vTexCoord;

    void main()
    {
        vTexCoord = aTexCoord;
        gl_Position = uProjection * uView * uModel * vec4(aPosition.x, aPosition.y, 0, 1.0);
    }
    ";

    private void TryInitOpenGl(GlInterface GL)
    {
        CheckError(GL);

        _shaderProgram = new ShaderProgram(GL, GlVersion.Type, VertexShaderSource, FragmentShaderSource);
        var positionLocation = _shaderProgram.GetAttribLocation("aPosition");
        var texCoordLocation = _shaderProgram.GetAttribLocation("aTexCoord");
        _shaderProgram.Compile();

        _vertexBuffer = new VertexBuffer.Builder(GL, _vertices, 4)
            .AttributeBinding(positionLocation, 2, 0)
            .AttributeBinding(texCoordLocation, 2, 2)
            .Build();

        _indicesBuffer = new IndicesBuffer(GL, _indices);

        _texture = new Texture(GL);

        var image = Image.Load<Rgba32>("../../../Assets/texture.jpg");

        _imageWidth = image.Width;
        _imageHeight = image.Height;
        _widthToHeight = _imageWidth / _imageHeight;

        var pixels = new byte[image.Width * 4 * image.Height];
        image.CopyPixelDataTo(pixels);
        image.Dispose();

        _texture.SetPixels(pixels, _imageWidth, _imageHeight);
    }

    protected override void OnOpenGlInit(GlInterface GL, int fb)
    {
        try
        {
            TryInitOpenGl(GL);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void TryToRender(GlInterface GL)
    {
        RecalculateImageSize();

        _indicesBuffer.Use();
        _vertexBuffer.Use();
        _texture.Use();
        _shaderProgram.Use();

        _shaderProgram.SetUniformMatrix4x4("uProjection", _projectionMatrix);
        _shaderProgram.SetUniformMatrix4x4("uView", _viewMatrix);
        _shaderProgram.SetUniformMatrix4x4("uModel", _modelMatrix);

        GL.DrawElements(GL_TRIANGLES, _indices.Length, GL_UNSIGNED_SHORT, IntPtr.Zero);
        CheckError(GL);
    }
    
    protected override void OnOpenGlRender(GlInterface GL, int fb)
    {
        GL.ClearColor(0, 0, 0, 0);
        GL.Clear(GL_COLOR_BUFFER_BIT);

        GL.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);

        try
        {
            TryToRender(GL);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void RecalculateImageSize()
    {
        if (_imageWidth > _imageHeight)
        {
            RecalculateImageWidth();

            if (_imageHeight <= (float)Bounds.Height) return;

            RecalculateImageHeight();

            return;
        }

        RecalculateImageHeight();

        if (_imageWidth <= (float)Bounds.Width) return;

        RecalculateImageWidth();
    }

    private void RecalculateImageWidth()
    {
        _imageWidth = (float)Bounds.Width;
        _imageHeight = _imageWidth / _widthToHeight;
    }

    private void RecalculateImageHeight()
    {
        _imageHeight = (float)Bounds.Height;
        _imageWidth = _imageHeight * _widthToHeight;
    }
    
    protected override void OnOpenGlDeinit(GlInterface GL, int fb)
    {
        _indicesBuffer.Destroy();
        _vertexBuffer.Destroy();
        _shaderProgram.Destroy();
    }

    private void CheckError(GlInterface gl)
    {
        int err;
        while ((err = gl.GetError()) != GL_NO_ERROR) Console.WriteLine($"{err}");
    }
}