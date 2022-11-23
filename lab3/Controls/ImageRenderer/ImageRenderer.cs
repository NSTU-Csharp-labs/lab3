using System;
using System.Runtime.InteropServices;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using static Avalonia.OpenGL.GlConsts;

namespace lab3.Controls.ImageRenderer;

public class ImageRenderer : OpenGlControlBase
{
    private int _vertexShader;
    private int _fragmentShader;
    private int _shaderProgram;
    private int _vertexBufferObject;
    private int _vertexArrayObject;

    private float[] _vertices = new[]
    {
        -0.5f, -0.5f, 0.0f, // Left  
        0.5f, -0.5f, 0.0f, // Right 
        0.0f, 0.5f, 0.0f // Top   
    };

    protected override unsafe void OnOpenGlInit(GlInterface GL, int fb)
    {
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
            GL.BufferData(GL_ARRAY_BUFFER, new IntPtr(_vertices.Length * sizeof(float)),
                new IntPtr(pdata), GL_STATIC_DRAW);
        
        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);
        CheckError(GL, 60);
        
        GL.VertexAttribPointer(positionLocation, 3, GL_FLOAT, 0, 3, IntPtr.Zero);
        GL.EnableVertexAttribArray(positionLocation);
        CheckError(GL, 64);
    }


    protected override void OnOpenGlRender(GlInterface GL, int fb)
    {
        GL.ClearColor(0, 0, 0, 1);
        GL.Clear(GL_COLOR_BUFFER_BIT);
        
        GL.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
        
        GL.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
        GL.BindVertexArray(_vertexArrayObject);
        GL.UseProgram(_shaderProgram);
        
        CheckError(GL, 73);
        
        GL.DrawArrays(GL_TRIANGLES, 0, new IntPtr(3));
        
        GL.BindVertexArray(0);
        
        CheckError(GL, 85);
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
attribute vec3 aPosition;

void main()
{
    gl_Position = vec4(aPosition.x, aPosition.y, aPosition.z, 1.0);
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