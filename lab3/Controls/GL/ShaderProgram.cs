using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Avalonia.OpenGL;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Svg;

namespace lab3.Controls.GL;

public class ShaderProgramCompiler
{
    private uint _lastAttributeIndex;

    private readonly List<(uint, string)> _attributeBindings;

    private List<Filter> _filters;

    public ShaderProgramCompiler()
    {
        _filters = new List<Filter>();
        _lastAttributeIndex = 0;
        _attributeBindings = new List<(uint, string)>();
    }

    public ShaderProgramCompiler BindAttribLocation(string name, out uint index)
    {
        index = _lastAttributeIndex;
        
        _attributeBindings.Add((_lastAttributeIndex, name));
        _lastAttributeIndex += 1;

        return this;
    }

    public ShaderProgramCompiler AddFilters(IEnumerable<Filter> filters)
    {
        _filters.AddRange(filters);
        return this;
    }

    public ShaderProgram Compile()
    {
        var vertexShader = CompileShader(ShaderType.VertexShader, ShadersSources.Vertex);
        var t = ShadersSources.BuildFragment(_filters);
        
        var fragmentShader = CompileShader(ShaderType.FragmentShader, ShadersSources.BuildFragment(_filters));
        var program = OpenTK.Graphics.OpenGL.GL.CreateProgram();

        foreach (var (location, name) in _attributeBindings)
        {
            OpenTK.Graphics.OpenGL.GL.BindAttribLocation(program, location, name);
            OpenGlUtils.CheckError();
        }

        OpenTK.Graphics.OpenGL.GL.AttachShader(program, vertexShader);
        OpenTK.Graphics.OpenGL.GL.AttachShader(program, fragmentShader);

        OpenGlUtils.CheckError();

        OpenTK.Graphics.OpenGL.GL.LinkProgram(program);
        OpenTK.Graphics.OpenGL.GL.GetProgramInfoLog(program, out var errorMessage);

        if (errorMessage.Length != 0)
            throw new OpenGlException($"OpenGl: link program error. {errorMessage}");

        return new ShaderProgram(program, vertexShader, fragmentShader);
    }

    private ShaderHandle CompileShader(ShaderType type, string sourceCode)
    {
        var shader = OpenTK.Graphics.OpenGL.GL.CreateShader(type);
        OpenTK.Graphics.OpenGL.GL.ShaderSource(shader, sourceCode);
        OpenTK.Graphics.OpenGL.GL.CompileShader(shader);

        OpenGlUtils.CheckError();

        OpenTK.Graphics.OpenGL.GL.GetShaderInfoLog(shader, out var errorMessage);

        if (errorMessage.Length != 0)
            throw new OpenGlException(
                $"OpenGl: add shader error. {errorMessage} fragment type: {type.ToString()}");

        return shader;
    }
}

public class ShaderProgram : IDisposable
{
    private readonly ProgramHandle _program;
    private readonly ShaderHandle _vertexShader;
    private readonly ShaderHandle _fragmentShader;

    public ShaderProgram(ProgramHandle program, ShaderHandle vertexShader,
        ShaderHandle fragmentShader)
    {
        _program = program;
        _vertexShader = vertexShader;
        _fragmentShader = fragmentShader;
    }

    public IDisposable Use()
    {
        OpenTK.Graphics.OpenGL.GL.UseProgram(_program);
        return new DisposableUsing(() =>
        {
            OpenTK.Graphics.OpenGL.GL.UseProgram(ProgramHandle.Zero);
        });
    }

    public void SetUniformMatrix4X4(string name, Matrix4x4 matrix)
    {
        var location = OpenTK.Graphics.OpenGL.GL.GetUniformLocation(_program, name);
        OpenTK.Graphics.OpenGL.GL.UniformMatrix4f(location, false, matrix.MapToOpenTKMatrix());

        OpenGlUtils.CheckError();
    }

    public void SetUniformBool(string name, bool flag)
    {
        SetUniformFloat(name, flag switch
        {
            true => 1f,
            false => 0f
        });
    }

    public void SetUniformFloat(string name, float value)
    {
        var location = OpenTK.Graphics.OpenGL.GL.GetUniformLocation(_program, name);
        OpenTK.Graphics.OpenGL.GL.Uniform1f(location, value);

        OpenGlUtils.CheckError();
    }

    public void Dispose()
    {
        OpenTK.Graphics.OpenGL.GL.DeleteProgram(_program);
        OpenTK.Graphics.OpenGL.GL.DeleteShader(_vertexShader);
        OpenTK.Graphics.OpenGL.GL.DeleteShader(_fragmentShader);
    }
}

public static class Matrix4x4Extensions
{
    public static Matrix4 MapToOpenTKMatrix(this Matrix4x4 matrix)
    {
        return new Matrix4(
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
        );
    }
}

static class ShadersSources
{
    public static string BuildFragment(IEnumerable<Filter> filters)
    {
        return $@"
    #version 330 core
    
    in vec2 texCoord;
    uniform sampler2D uTexture;

    out vec4 oColor;

    {string.Join("\n", filters.Select(f => $"{f.SourceCode}\n"))}

    void main()
    {{
        vec4 color = texture(uTexture, texCoord); 
        {string.Join("\n", filters.Select(f => $"color = {f.Name}(color);\n"))} 
        oColor = color;   
    }}
    ";
    } 
    
    public const string Fragment = @"
    #version 330 core
    
    in vec2 texCoord;
    uniform sampler2D uTexture;

    out vec4 oColor;

    void main()
    {
        vec4 color = texture(uTexture, texCoord); 
        oColor = color;   
    }
    ";

    public const string Vertex = @"
    #version 330 core

    in vec2 iPosition;
    in vec2 iTexCoord;
    uniform mat4 uScale;

    out vec2 texCoord;

    void main()
    {
        texCoord = iTexCoord;
        gl_Position = uScale * vec4(iPosition.x, iPosition.y, 0, 1.0);
    }
    ";
}