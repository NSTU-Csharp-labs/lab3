using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static Avalonia.OpenGL.GlConsts;
using Image = SixLabors.ImageSharp.Image;

namespace lab3.Controls.GL;

public class ImageRenderer : OpenGlControlBase
{
    const int CUSTOM_GL_STREAM_DRAW = 0x88E0;

    public const int CUSTOM_GL_TEXTURE_WRAP_S = 0x2802;
    public const int CUSTOM_GL_TEXTURE_WRAP_T = 0x2803;
    public const int CUSTOM_GL_CLAMP_TO_EDGE = 0x812F;


    private ShaderProgram _shaderProgram;
    private Texture _texture;
    
    private int _vertexBufferObject;
    private int _vertexArrayObject;
    private int _indicesBufferObject;

    private float[] _vertices =
    {
        1f, 1f, -1, -1,
        1f, -1f, -1, 1,
        -1f, -1f, 1, 1,
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

    private void UpdateVertexes()
    {
        // _vertices[2] = _vertices[0] = -(float)Bounds.Width / 2;
        // _vertices[3] = _vertices[1] = -(float)Bounds.Height / 2;
        //
        // _vertices[6] = _vertices[4] = -(float)Bounds.Width / 2;
        // _vertices[7] = _vertices[5] = (float)Bounds.Height / 2;
        //
        // _vertices[10] = _vertices[8] = (float)Bounds.Width / 2;
        // _vertices[11] = _vertices[9] = -(float)Bounds.Height / 2;
        //
        // _vertices[14] = _vertices[12] = (float)Bounds.Width / 2;
        // _vertices[15] = _vertices[13] = (float)Bounds.Height / 2;


        // _vertices[2] = -1;
        // _vertices[3] =  -1;
        //
        // _vertices[6] = -1;
        // _vertices[7] = 1;
        //
        // _vertices[10] = 1;
        // _vertices[11] = -1;
        //
        // _vertices[14] = 1;
        // _vertices[15] = 1;
        //
        //
        //
        // _vertices[2] = _vertices[0] = 1;
        // _vertices[3] = _vertices[1] = 1;
        //
        // _vertices[6] = _vertices[4] = 1;
        // _vertices[7] = _vertices[5] = 0;
        //
        // _vertices[10] = _vertices[8] = 0;
        // _vertices[11] = _vertices[9] = 0;
        //
        // _vertices[14] = _vertices[12] = 0;
        // _vertices[15] = _vertices[13] = 1;
    }

    private float _imageWidth;
    private float _imageHeight;

    private void OnSizeChange(object? sender, SizeChangedEventArgs e) => UpdateVertexes();

    private unsafe void TryInitOpenGl(GlInterface GL)
    {
        UpdateVertexes();
        CheckError(GL);


        _shaderProgram = new ShaderProgram(GL, GlVersion.Type, VertexShaderSource, FragmentShaderSource);
        int positionLocation = _shaderProgram.GetAttribLocation("aPosition");
        int texCoordLocation = _shaderProgram.GetAttribLocation("aTexCoord");
        _shaderProgram.Compile();

        _vertexBufferObject = GL.GenBuffer();

        GL.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
        CheckError(GL);

        fixed (void* pdata = _vertices)
            GL.BufferData(
                GL_ARRAY_BUFFER, new IntPtr(_vertices.Length * sizeof(float)),
                new IntPtr(pdata), CUSTOM_GL_STREAM_DRAW
            );

        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);
        CheckError(GL);

        GL.VertexAttribPointer(positionLocation, 2, GL_FLOAT, 0, 4 * sizeof(float), IntPtr.Zero);
        GL.EnableVertexAttribArray(positionLocation);

        GL.VertexAttribPointer(texCoordLocation, 2, GL_FLOAT, 0, 4 * sizeof(float), new IntPtr(2 * sizeof(float)));
        GL.EnableVertexAttribArray(texCoordLocation);

        CheckError(GL);
        _indicesBufferObject = GL.GenBuffer();
        GL.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _indicesBufferObject);
        CheckError(GL);

        fixed (void* pdata = _indices)
            GL.BufferData(
                GL_ELEMENT_ARRAY_BUFFER, new IntPtr(_indices.Length * sizeof(ushort)),
                new IntPtr(pdata), GL_STATIC_DRAW
            );
        _texture = new Texture(GL);

        
        var image = Image.Load<Rgba32>("../../../Assets/texture.jpg");
        
        _imageHeight = image.Height;
        _imageWidth = image.Width;
        
        var pixels = new byte[image.Width * 4 * image.Height];
        image.CopyPixelDataTo(pixels);
        image.Dispose();
        
        _texture.SetPixels(pixels, _imageWidth, _imageHeight);
        SizeChanged += OnSizeChange;
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
        GL.ClearColor(0.23f, 0.23f, 0.23f, 1);
        GL.Clear(GL_COLOR_BUFFER_BIT);

        GL.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);

        GL.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
        GL.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _indicesBufferObject);

        fixed (void* pdata = _vertices)
            GL.BufferData(
                GL_ARRAY_BUFFER, new IntPtr(_vertices.Length * sizeof(float)),
                new IntPtr(pdata), GL_STATIC_DRAW
            );

        _texture.Use();
        
        GL.BindVertexArray(_vertexArrayObject);
        
        _shaderProgram.Use();

        var projection = Matrix4x4.CreateOrthographicOffCenter(0, (float)Bounds.Width,
            0, (float)Bounds.Height, 0, 10);

        var view = Matrix4x4
            .CreateLookAt(
                new Vector3(0, 0, 1),
                new Vector3(),
                new Vector3(0, 1, 0)
            );

        var model = Matrix4x4.Multiply(
            Matrix4x4.CreateScale(_imageWidth, _imageHeight, 1),
            Matrix4x4.CreateReflection(new Plane(0, -1, 0, 0))
        );

        _shaderProgram.SetUniformMatrix4x4("uProjection", projection);
        _shaderProgram.SetUniformMatrix4x4("uView", view);
        _shaderProgram.SetUniformMatrix4x4("uModel", model);

        GL.DrawElements(GL_TRIANGLES, _indices.Length, GL_UNSIGNED_SHORT, IntPtr.Zero);
        CheckError(GL);

        GL.BindVertexArray(0);
        CheckError(GL);
    }

    protected override void OnOpenGlRender(GlInterface GL, int fb)
    {
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
        GL.BindBuffer(GL_ARRAY_BUFFER, 0);
        GL.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);

        GL.DeleteBuffer(_vertexBufferObject);
        GL.DeleteVertexArray(_vertexArrayObject);
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