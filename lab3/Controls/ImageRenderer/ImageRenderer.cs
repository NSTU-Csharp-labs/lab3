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
    
    private float[] _vertices = new [] {
        -0.5f, -0.5f, 0.0f,     // Left  
        0.5f, -0.5f, 0.0f,      // Right 
        0.0f,  0.5f, 0.0f       // Top   
    };
    
    
    public ImageRenderer()
    {
        AffectsRender<ImageRenderer>();
    }

    protected override unsafe void OnOpenGlInit(GlInterface GL, int fb)
    {
            CheckError(GL);
        
            _vertexShader = GL.CreateShader(GL_VERTEX_SHADER);
            _fragmentShader = GL.CreateShader(GL_FRAGMENT_SHADER);
        
            Console.WriteLine(GL.CompileShaderAndGetError(_vertexShader, VertexShaderSource));
            Console.WriteLine(GL.CompileShaderAndGetError(_fragmentShader, FragmentShaderSource));
        
            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, _vertexShader);
            GL.AttachShader(_shaderProgram, _fragmentShader);
        
            _vertexBufferObject = GL.GenBuffer();
            
            GL.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
            CheckError(GL);
            
            fixed (void* pdata = _vertices)
                GL.BufferData(GL_ARRAY_BUFFER, new IntPtr(_vertices.Length * sizeof(float)),
                    new IntPtr(pdata), GL_STATIC_DRAW);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            CheckError(GL);
            
            GL.VertexAttribPointer(0, 3, GL_FLOAT, 0, 3, IntPtr.Zero);
            GL.EnableVertexAttribArray(0);
            CheckError(GL);
    }

    
 
    
    protected override unsafe void OnOpenGlRender(GlInterface GL, int fb)
    {
        GL.ClearColor(0, 0, 0, 1);
        GL.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        GL.Enable(GL_DEPTH_TEST);
        
        GL.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
        
        GL.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
        GL.BindVertexArray(_vertexArrayObject);
        GL.UseProgram(_shaderProgram);

        CheckError(GL);
        
        GL.DrawArrays(GL_TRIANGLES, 0, new IntPtr(3));
        
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
        GL.DeleteProgram(_shaderProgram);
        GL.DeleteShader(_fragmentShader);
        GL.DeleteShader(_vertexShader);
    }
    
    
    private void CheckError(GlInterface gl)
    {
        int err;
        while ((err = gl.GetError()) != GL_NO_ERROR)
            Console.WriteLine(err);
    }

    private string FragmentShaderSource => GetShader(true, @"
    out vec4 color;

    void main()
    {
	    color = vec4(1.0f, 0.5f, 0.2f, 1.0f);
    }

");

    private string VertexShaderSource => GetShader(false, @"
    layout (location = 0) in vec3 position;

    void main()
    {
        gl_Position = vec4(position.x, position.y, position.z, 1.0);
    }
");
    
    
    
    
    private string GetShader(bool fragment, string shader)
    {
        var version = (GlVersion.Type == GlProfileType.OpenGL ?
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 150 : 120 :
            100);
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

