using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Platform.Interop;
using Avalonia.Threading;
using static Avalonia.OpenGL.GlConsts;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;


namespace lab3.Controls.ImageRenderer;

public class ImageRenderer : OpenGlControlBase
{
    
    private int _vertexShader;
    private int _fragmentShader;
    private int _shaderProgram;
    private int _vertexBufferObject;
    private int _indexBufferObject;
    private int _vertexArrayObject;
    
    
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
    }
    
    private readonly Vertex[] _points;
    private readonly float _minY;
    private readonly float _maxY;
    
    
    private List<float> vertices = new List<float>(){
        -0.5f, -0.5f, 0.0f, // Left  
        0.5f, -0.5f, 0.0f, // Right 
        0.0f,  0.5f, 0.0f  // Top   
    };
    
    
    public ImageRenderer()
    {
        AffectsRender<ImageRenderer>();
    }

    protected override unsafe void OnOpenGlInit(GlInterface GL, int fb)
    {
            CheckError(GL);
            var Info = $"Renderer: {GL.GetString(GL_RENDERER)} Version: {GL.GetString(GL_VERSION)}";
        
            _vertexShader = GL.CreateShader(GL_VERTEX_SHADER);
            _fragmentShader = GL.CreateShader(GL_FRAGMENT_SHADER);
        
            Console.WriteLine(GL.CompileShaderAndGetError(_vertexShader, VertexShaderSource));
            Console.WriteLine(GL.CompileShaderAndGetError(_fragmentShader, FragmentShaderSource));
        
            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, _vertexShader);
            GL.AttachShader(_shaderProgram, _fragmentShader);
        
            const int positionLocation = 0;
            const int normalLocation = 1;

            _vertexBufferObject = GL.GenBuffer();
            
            GL.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
            CheckError(GL);
            
            var vertexSize = Marshal.SizeOf<Vertex>();
            fixed (void* pdata = _points)
                GL.BufferData(GL_ARRAY_BUFFER, new IntPtr(_points.Length * vertexSize),
                    new IntPtr(pdata), GL_STATIC_DRAW);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            CheckError(GL);
            
            GL.VertexAttribPointer(0, 3, GL_FLOAT, 0, 3, IntPtr.Zero);
            GL.EnableVertexAttribArray(0);
            CheckError(GL);
    }

    
 
    
    protected override void OnOpenGlRender(GlInterface GL, int fb)
    {
     
        
    }
   
    
    protected override void OnOpenGlDeinit(GlInterface GL, int fb)
    {
        GL.BindBuffer(GL_ARRAY_BUFFER, 0);
        GL.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);

        GL.DeleteBuffer(_vertexBufferObject);
        GL.DeleteBuffer(_indexBufferObject);
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

