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

    private int _vertexShader;
    private int _fragmentShader;
    private int _texture;

    private ShaderProgram _shaderProgram;

    private int _vertexBufferObject;
    private int _vertexArrayObject;
    private int _indicesBufferObject;

    private float[] _vertices =
    {
        0f, 0f, 0, 0,
        0f, 0f, 0, 0,
        0f, 0f, 0, 0,
        0f, 0f, 0, 0,
    };

    private ushort[] _indices =
    {
        0, 1, 2,
        1, 3, 2,
    };

    private void UpdateVertexes()
    {
        _vertices[2] = _vertices[0] = -(float)Bounds.Width / 2;
        _vertices[3] = _vertices[1] = -(float)Bounds.Height / 2;

        _vertices[6] = _vertices[4] = -(float)Bounds.Width / 2;
        _vertices[7] = _vertices[5] = (float)Bounds.Height / 2;

        _vertices[10] = _vertices[8] = (float)Bounds.Width / 2;
        _vertices[11] = _vertices[9] = -(float)Bounds.Height / 2;

        _vertices[14] = _vertices[12] = (float)Bounds.Width / 2;
        _vertices[15] = _vertices[13] = (float)Bounds.Height / 2;
    }

    private void OnSizeChange(object? sender, SizeChangedEventArgs e) => UpdateVertexes();

    protected override unsafe void OnOpenGlInit(GlInterface GL, int fb)
    {
        UpdateVertexes();
        CheckError(GL);
        var positionLocation = 0;
        var texCoordLocation = 0;

        try
        {
            _shaderProgram = new ShaderProgram(GL, GlVersion.Type);
            _shaderProgram.AddShader(GL_VERTEX_SHADER, false, VertexShaderSource);
            _shaderProgram.AddShader(GL_FRAGMENT_SHADER, true, FragmentShaderSource);
            _shaderProgram.CreateShaderProgram();

            _shaderProgram.BindAttribLocationString(positionLocation, "aPosition");
            _shaderProgram.BindAttribLocationString(texCoordLocation, "aTexCoord");
            _shaderProgram.LinkShaderProgram();
        }
        catch (ShaderProgramException ex)
        {
            Console.WriteLine(ex.Message);
        }

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

        GL.GenTextures(1, (int*)(_texture));
        _texture = GL.GenTexture();
        GL.BindTexture(GL_TEXTURE_2D, _texture);
        GL.TexParameteri(GL_TEXTURE_2D, CUSTOM_GL_TEXTURE_WRAP_S, CUSTOM_GL_CLAMP_TO_EDGE);
        GL.TexParameteri(GL_TEXTURE_2D, CUSTOM_GL_TEXTURE_WRAP_T, CUSTOM_GL_CLAMP_TO_EDGE);
        GL.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
        GL.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

        var image = Image.Load<Rgba32>("/home/danilka108/RiderProjects/lab3/lab3/Assets/texture.jpg");

        var pixels = new byte[image.Width * 4 * image.Height];
        image.CopyPixelDataTo(pixels);
        image.Dispose();

        CheckError(GL);


        fixed (byte* p = pixels)
        {
            GL.TexImage2D(
                GL_TEXTURE_2D,
                0,
                GL_RGBA,
                image.Width,
                image.Height,
                0,
                GL_RGBA,
                GL_UNSIGNED_BYTE,
                new IntPtr(p));
        }

        CheckError(GL);

        GL.BindTexture(GL_TEXTURE_2D, 0);

        // GL.TexParameteri(GL_TEXTURE_2D, );
        // GL.TexParameteri(GL_TEXTURE_2D, GL_ACTIVE_TEXTURE, );

        SizeChanged += OnSizeChange;
    }

    protected override unsafe void OnOpenGlRender(GlInterface GL, int fb)
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

        GL.BindTexture(GL_TEXTURE_2D, _texture);
        GL.BindVertexArray(_vertexArrayObject);
        GL.UseProgram(_shaderProgram.Link);

        var projection = Matrix4x4.CreateOrthographicOffCenter(-(float)Bounds.Width / 2, (float)Bounds.Width / 2,
            -(float)Bounds.Height / 2, (float)Bounds.Height / 2, 0, 10);

        var view = Matrix4x4
            .CreateLookAt(new Vector3(0, 0, 5), new Vector3(), new Vector3(0, 1, 0));
        var model = Matrix4x4.CreateScale(1);

        var projectionLoc = GL.GetUniformLocationString(_shaderProgram.Link, "uProjection");
        var viewLoc = GL.GetUniformLocationString(_shaderProgram.Link, "uView");
        var modelLoc = GL.GetUniformLocationString(_shaderProgram.Link, "uModel");

        GL.UniformMatrix4fv(projectionLoc, 1, false, &projection);
        GL.UniformMatrix4fv(viewLoc, 1, false, &view);
        GL.UniformMatrix4fv(modelLoc, 1, false, &model);

        CheckError(GL);

        GL.DrawElements(GL_TRIANGLES, _indices.Length, GL_UNSIGNED_SHORT, IntPtr.Zero);

        CheckError(GL);

        GL.BindVertexArray(0);

        CheckError(GL);
    }

    protected override void OnOpenGlDeinit(GlInterface GL, int fb)
    {
        GL.BindBuffer(GL_ARRAY_BUFFER, 0);
        GL.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);

        GL.DeleteBuffer(_vertexBufferObject);
        GL.DeleteVertexArray(_vertexArrayObject);
        GL.DeleteProgram(_shaderProgram.Link);
        GL.DeleteShader(_fragmentShader);
        GL.DeleteShader(_vertexShader);
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