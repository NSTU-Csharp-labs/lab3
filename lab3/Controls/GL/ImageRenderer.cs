using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static Avalonia.OpenGL.GlConsts;
using Image = SixLabors.ImageSharp.Image;

namespace lab3.Controls.GL;

public class ImageRenderer : OpenGlControlBase
{
    private ShaderProgram _shaderProgram;
    private Texture _texture;
    private VertexBuffer _vertexBuffer;
    private IndicesBuffer _indicesBuffer;

    private float[] _vertices =
    {
        1f, 1f, 0, -1,
        1f, -1f, 0, 0,
        -1f, -1f, 1, 0,
        -1f, 1f, 1, -1,

        // 1f,  1f,   1,  1,
        // 1f, -1f,   1, -1, 
        // -1f, -1f,  -1, -1,
        // -1f,  1f,  -1,  1,
    };

    private ushort[] _indices =
    {
        0, 1, 3,
        1, 2, 3,
    };
    
    private float _imageWidth;
    private float _imageHeight;

    private float _widthToHeight;

    private unsafe void TryInitOpenGl(GlInterface GL)
    {
        CheckError(GL);

        _shaderProgram = new ShaderProgram(GL, GlVersion.Type, VertexShaderSource, FragmentShaderSource);
        int positionLocation = _shaderProgram.GetAttribLocation("aPosition");
        int texCoordLocation = _shaderProgram.GetAttribLocation("aTexCoord");
        _shaderProgram.Compile();
        
        _vertexBuffer = new VertexBuffer(GL, _vertices, 4);
        _vertexBuffer.BindAttribute(positionLocation, 2, 0);
        _vertexBuffer.BindAttribute(texCoordLocation, 2, 2);


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

    protected override unsafe void OnOpenGlInit(GlInterface GL, int fb)
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

    private unsafe void TryToRender(GlInterface GL)
    {
        RecalculateImageSize();
        
        _indicesBuffer.Use();
        _vertexBuffer.Use();
        _texture.Use();
        _shaderProgram.Use();

        var projection = Matrix4x4.CreateOrthographicOffCenter(
            -(float)Bounds.Width / 2,
            (float)Bounds.Width / 2,
            -(float)Bounds.Height / 2,
            (float)Bounds.Height / 2,
            0,
            10);

        var view = Matrix4x4
            .CreateLookAt(
                new Vector3(_imageWidth / 2, _imageHeight / 2, 1),
                new Vector3(_imageWidth / 2, _imageHeight / 2, 0),
                new Vector3(0, 1, 0)
            );

        var model = Matrix4x4.Multiply(
            Matrix4x4.CreateScale(
                _imageWidth,
                _imageHeight,
                1),
            Matrix4x4.CreateReflection(
                new Plane(0, 1, 0, 0))
        );

        _shaderProgram.SetUniformMatrix4x4("uProjection", projection);
        _shaderProgram.SetUniformMatrix4x4("uView", view);
        _shaderProgram.SetUniformMatrix4x4("uModel", model);

        GL.DrawElements(GL_TRIANGLES, _indices.Length, GL_UNSIGNED_SHORT, IntPtr.Zero);
        CheckError(GL);
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

    protected override void OnOpenGlDeinit(GlInterface GL, int fb)
    {
        _indicesBuffer.Destroy();
        _vertexBuffer.Destroy();
        _shaderProgram.Destroy();
    }

    private void CheckError(GlInterface gl)
    {
        int err;
        while ((err = gl.GetError()) != GL_NO_ERROR)
        {
            Console.WriteLine($"{err}");
        }
    }

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
}