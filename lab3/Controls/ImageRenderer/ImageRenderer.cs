using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using static Avalonia.OpenGL.GlConsts;

namespace lab3.Controls.ImageRenderer;

public class ImageRenderer : OpenGlControlBase
{
    const int CUSTOM_GL_STREAM_DRAW = 0x88E0;
    private int _vertexShader;
    private int _fragmentShader;
    private int _shaderProgram;
    private int _vertexBufferObject;
    private int _vertexArrayObject;
    private int _indicesBufferObject;

    private float[] _vertices = {
        0f, 0f, 
        0f, 0f,
        0f, 0f,
        0f, 0f,
    };

    private ushort[] _indices = {
        0, 1, 2,
        1, 3, 2,
    };

    private void UpdateVertexes()
    {
        _vertices[0] = -(float)Bounds.Width / 2;
        _vertices[1] = -(float)Bounds.Height / 2;
        
        _vertices[2] = -(float)Bounds.Width / 2;
        _vertices[3] = (float)Bounds.Height / 2;
        
        _vertices[4] = (float)Bounds.Width / 2;
        _vertices[5] = -(float)Bounds.Height / 2;

        _vertices[6] = (float)Bounds.Width / 2;
        _vertices[7] = (float)Bounds.Height / 2;
    }
    
    private void OnSizeChange(object? sender, SizeChangedEventArgs e) => UpdateVertexes();
    
    protected override unsafe void OnOpenGlInit(GlInterface GL, int fb)
    {
        UpdateVertexes();
        CheckError(GL, 32);

        _vertexShader = GL.CreateShader(GL_VERTEX_SHADER);
        _fragmentShader = GL.CreateShader(GL_FRAGMENT_SHADER);

        Console.WriteLine(GL.CompileShaderAndGetError(_vertexShader, VertexShaderSource));
        Console.WriteLine(GL.CompileShaderAndGetError(_fragmentShader, FragmentShaderSource));

        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, _vertexShader);
        GL.AttachShader(_shaderProgram, _fragmentShader);
        
        var positionLocation = 0;
        GL.BindAttribLocationString(_shaderProgram, positionLocation, "aPosition");
        
        Console.WriteLine(GL.LinkProgramAndGetError(_shaderProgram));

        CheckError(GL, 47);

        _vertexBufferObject = GL.GenBuffer();

        GL.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
        CheckError(GL, 52);

        fixed (void* pdata = _vertices)
            GL.BufferData(
                GL_ARRAY_BUFFER, new IntPtr(_vertices.Length * sizeof(float)),
                new IntPtr(pdata), CUSTOM_GL_STREAM_DRAW
                );

        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);
        CheckError(GL, 60);
        
        GL.VertexAttribPointer(positionLocation, 2, GL_FLOAT, 0, 0, IntPtr.Zero);
        GL.EnableVertexAttribArray(positionLocation);
        
        CheckError(GL, 89);
        _indicesBufferObject = GL.GenBuffer();
        GL.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _indicesBufferObject);
        CheckError(GL, 92);
        
        fixed (void* pdata = _indices)
            GL.BufferData(
                GL_ELEMENT_ARRAY_BUFFER, new IntPtr(_indices.Length * sizeof(ushort)),
                new IntPtr(pdata), GL_STATIC_DRAW
            );
        
        base.SizeChanged += OnSizeChange;

    }

    


    protected override unsafe void OnOpenGlRender(GlInterface GL, int fb)
    {
        GL.ClearColor(0.23f,0.23f, 0.23f, 1);
        GL.Clear(GL_COLOR_BUFFER_BIT);

        GL.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
        

        GL.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
        GL.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _indicesBufferObject);
        
        fixed (void* pdata = _vertices)
            GL.BufferData(
                GL_ARRAY_BUFFER, new IntPtr(_vertices.Length * sizeof(float)),
                new IntPtr(pdata), GL_STATIC_DRAW
            );

        GL.BindVertexArray(_vertexArrayObject);
        GL.UseProgram(_shaderProgram);

        var projection = Matrix4x4.CreateOrthographicOffCenter(-(float)Bounds.Width / 2, (float)Bounds.Width / 2,
            -(float)Bounds.Height / 2, (float)Bounds.Height / 2, 0, 10);
        
        var view = Matrix4x4
            .CreateLookAt(new Vector3(0, 0, 5), new Vector3(), new Vector3(0, 1, 0));
        var model = Matrix4x4.CreateScale(1);

        var projectionLoc = GL.GetUniformLocationString(_shaderProgram, "uProjection");
        var viewLoc = GL.GetUniformLocationString(_shaderProgram, "uView");
        var modelLoc = GL.GetUniformLocationString(_shaderProgram, "uModel");
        
        GL.UniformMatrix4fv(projectionLoc, 1, false, &projection);
        GL.UniformMatrix4fv(viewLoc, 1, false, &view);
        GL.UniformMatrix4fv(modelLoc, 1, false, &model);

        CheckError(GL, 85);

        // GL.DrawArrays(GL_TRIANGLES, 0, new IntPtr(3));
        GL.DrawElements(GL_TRIANGLES, _indices.Length, GL_UNSIGNED_SHORT, IntPtr.Zero);
        
        CheckError(GL, 89);

        GL.BindVertexArray(0);

        CheckError(GL, 93);
    }

    protected override void OnOpenGlDeinit(GlInterface GL, int fb)
    {
        Console.WriteLine(VertexShaderSource);
        Console.WriteLine(FragmentShaderSource);
        GL.BindBuffer(GL_ARRAY_BUFFER, 0);
        GL.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);

        GL.DeleteBuffer(_vertexBufferObject);
        GL.DeleteVertexArray(_vertexArrayObject);
        GL.DeleteProgram(_shaderProgram);
        GL.DeleteShader(_fragmentShader);
        GL.DeleteShader(_vertexShader);
    }

    private void CheckError(GlInterface gl, int num)
    {
        int err;
        while ((err = gl.GetError()) != GL_NO_ERROR)
        {
            Console.WriteLine($"[{num}] {err}");
        }
    }

    private string FragmentShaderSource => GetShader(true, @"
    void main()
    {
        gl_FragColor = vec4(1.0, 0.5, 0.2, 1.0);
    }
    ");

    private string VertexShaderSource => GetShader(false, @"
    attribute vec2 aPosition;
    uniform mat4 uProjection;
    uniform mat4 uView;
    uniform mat4 uModel;

    void main()
    {
        gl_Position = uProjection * uView * uModel * vec4(aPosition.x, aPosition.y, 0, 1.0);
    }
    ");


    private string GetShader(bool fragment, string shader)
    {
        var version = GlVersion.Type == GlProfileType.OpenGL
            ? RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 150 : 120
            : 100;
        var data = "#version " + version + "\n";

        if (GlVersion.Type == GlProfileType.OpenGLES)
            data += "precision mediump float;\n";
        if (version >= 150)
        {
            shader = shader.Replace("attribute", "in");
            if (fragment)
                shader = shader
                    .Replace("varying", "in")
                    .Replace("//DECLAREGLFRAG", "out vec4 outFragColor;")
                    .Replace("gl_FragColor", "outFragColor");
            else
                shader = shader.Replace("varying", "out");
        }

        data += shader;

        return data;
    }
}