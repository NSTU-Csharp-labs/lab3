using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using static Avalonia.OpenGL.GlConsts;

namespace lab3.Controls.GL;

public class ShaderProgram
{
    private GlInterface _gl;
    private GlProfileType _type;

    private List<int> _shaders;

    public int Link { get; private set; }

    public ShaderProgram(GlInterface gl, GlProfileType type)
    {
        _gl = gl;
        _type = type;
        _shaders = new List<int>();
        Link =  _gl.CreateProgram();
    }

    public void AddShader(int shaderType, bool fragment,string shaderCode)
    {
        var shader = _gl.CreateShader(shaderType);
        var returned = _gl.CompileShaderAndGetError(shader, GetShader(fragment, shaderCode));
        if (!CheckOpenGlExeption(returned))
        {
            throw new ShaderProgramException($"OpenGl: add shader error");
        }

        _shaders.Add(shader);

        CheckError();
    }

    public void CreateShaderProgram()
    {
        foreach (var shader in _shaders)
        {
            _gl.AttachShader(Link, shader);            
        }
        CheckError();
    }

    public void BindAttribLocationString(int index, string name)
    {
        _gl.BindAttribLocationString(Link, index, name);
    }

    public void LinkShaderProgram()
    {
        var returned = _gl.LinkProgramAndGetError(Link); 
        if (!CheckOpenGlExeption(returned))
        {
            throw new ShaderProgramException($"OpenGl: link shader program error");
        }

        CheckError();
    }

    private bool CheckOpenGlExeption(string? returned)
    {
        return (returned == null || returned.Length.Equals(0));
    }
    
    
    private string GetShader(bool fragment, string shader)
    {
        var version = _type == GlProfileType.OpenGL
            ? RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 150 : 120
            : 100;
        var data = "#version " + version + "\n";

        if (_type == GlProfileType.OpenGLES)
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
    
    private void CheckError()
    {
        int err;
        while ((err = _gl.GetError()) != GL_NO_ERROR)
        {
            throw new ShaderProgramException(Convert.ToString(err));
            Console.WriteLine($"{err}");
        }
    }
    
    
    
    
}


public class ShaderProgramException : Exception
{ 
    public ShaderProgramException(string message) : base(message){ }
}


