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

    private int _vertexShader;
    private int _fragmentShader;

    public int Link { get; private set; }

    public ShaderProgram(GlInterface gl, GlProfileType type,string vertexShaderSource, string fragmentShaderSource)
    {
        _gl = gl;
        _type = type;
        _vertexShader = AddShader(GL_VERTEX_SHADER, false, vertexShaderSource);
        _fragmentShader = AddShader(GL_FRAGMENT_SHADER, true, fragmentShaderSource);
        Link = _gl.CreateProgram();
        // CreateShaderProgram();
        // LinkShaderProgram();
    }

    private int AddShader(int shaderType, bool fragment, string shaderCode)
    {
        var shader = _gl.CreateShader(shaderType);
        var returned = _gl.CompileShaderAndGetError(shader, GetShader(fragment, shaderCode));
        if (!CheckOpenGlExeption(returned))
        {
            throw new ShaderProgramException(
                $"OpenGl: add shader error. {returned} is fragment: {fragment.ToString()} ");
        }
        CheckError();

        return shader;
    }

    public void Compile()
    {
        CreateShaderProgram();
        LinkShaderProgram();
    }
    

    private void CreateShaderProgram()
    {
        _gl.AttachShader(Link, _vertexShader);
        _gl.AttachShader(Link, _fragmentShader);
        CheckError();
    }

    public void Use()
    {
        _gl.UseProgram(Link);
    }


    public unsafe void SetUniformMatrix4x4( string name, Matrix4x4 matrix)
    {
        _gl.UniformMatrix4fv(_gl.GetUniformLocationString(Link, name), 1, false, &matrix);

    }
    
    
    public int GetAttribLocation( string name)
    {
        int index = 0;
        _gl.BindAttribLocationString(Link, index, name);
        CheckError();
        return index;
    }

    private void LinkShaderProgram()
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
        // var version = 330;
        var data = "#version " + 330 + "\n";

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
        }
    }

    public void Destroy()
    {
        _gl.DeleteProgram(Link);

        _gl.DeleteShader(_vertexShader);
        _gl.DeleteShader(_fragmentShader);
    }
}

public class ShaderProgramException : Exception
{
    public ShaderProgramException(string message) : base(message)
    {
    }
}