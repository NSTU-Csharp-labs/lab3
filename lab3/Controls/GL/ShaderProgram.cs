using System;
using System.Numerics;
using Avalonia.OpenGL;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Svg;

namespace lab3.Controls.GL;

public class ShaderProgram : IDisposable
{
    private readonly ShaderHandle _fragmentShader;
    private readonly ProgramHandle _program;
    private readonly ShaderHandle _vertexShader;
    private uint _lastAttributeIndex;

    public ShaderProgram()
    {
        _vertexShader = AddShader(ShaderType.VertexShader, VertexShader);
        _fragmentShader = AddShader(ShaderType.FragmentShader, FragmentShader);
        _program = OpenTK.Graphics.OpenGL.GL.CreateProgram();

        _lastAttributeIndex = 0;
    }

    private ShaderHandle AddShader(ShaderType type, string sourceCode)
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

    public void Compile()
    {
        OpenTK.Graphics.OpenGL.GL.AttachShader(_program, _vertexShader);
        OpenTK.Graphics.OpenGL.GL.AttachShader(_program, _fragmentShader);

        OpenGlUtils.CheckError();

        OpenTK.Graphics.OpenGL.GL.LinkProgram(_program);
        OpenTK.Graphics.OpenGL.GL.GetProgramInfoLog(_program, out var errorMessage);

        if (errorMessage.Length != 0)
            throw new OpenGlException($"OpenGl: link program error. {errorMessage}");
    }

    public void Use()
    {
        OpenTK.Graphics.OpenGL.GL.UseProgram(_program);
    }

    public void SetUniformMatrix4X4(string name, Matrix4x4 matrix)
    {
        var location = OpenTK.Graphics.OpenGL.GL.GetUniformLocation(_program, name);
        OpenTK.Graphics.OpenGL.GL.UniformMatrix4f(location, false, matrix.MapToOpenTKMatrix());
        
        OpenGlUtils.CheckError();
    }

    public void SetUniformBool(string name, bool flag)
    {
        var location = OpenTK.Graphics.OpenGL.GL.GetUniformLocation(_program, name);
        OpenTK.Graphics.OpenGL.GL.Uniform1f(location, flag switch
        {
            true => 1f,
            false => 0f,
        });
        
        OpenGlUtils.CheckError();
    }

    public void SetUniformFloat(string name, float value)
    {
        var location = OpenTK.Graphics.OpenGL.GL.GetUniformLocation(_program, name);
        OpenTK.Graphics.OpenGL.GL.Uniform1f(location, value);
        
        OpenGlUtils.CheckError();
    }

    public uint BindAttribLocation(string name)
    {
        OpenTK.Graphics.OpenGL.GL.BindAttribLocation(_program, _lastAttributeIndex, name);
        OpenGlUtils.CheckError();

        var index = _lastAttributeIndex;
        _lastAttributeIndex += 1;

        return index;
    }

    public void Dispose()
    {
        OpenTK.Graphics.OpenGL.GL.DeleteProgram(_program);
        OpenTK.Graphics.OpenGL.GL.DeleteShader(_vertexShader);
        OpenTK.Graphics.OpenGL.GL.DeleteShader(_fragmentShader);
    }

    private const string FragmentShader = @"
    #version 330 core

    varying vec2 vTexCoord;
    uniform sampler2D uTexture;
    
    uniform bool uBlackAndWhite;
    uniform bool uRed;
    uniform bool uBlue;
    uniform bool uGreen;
    uniform float uCornerRadius;

    vec4 EnableEffects(vec4 color);
    vec4 Green(vec4 color);
    vec4 Red(vec4 color);
    vec4 Blue(vec4 color);
    vec4 RoundCorners(vec4 color);

    void main()
    {
        vec4 color = texture(uTexture, vTexCoord); 
        if (uBlackAndWhite) {
            color = EnableEffects(color);
        } 
        if (uRed) {
            color = Red(color);
        }
        if (uGreen) {
            color = Green(color);
        } 
        if (uBlue) {
            color = Blue(color);
        }

        /*color = RoundCorners(color);*/

        gl_FragColor = color;   
    }

    vec4 RoundCorners(vec4 color)
    {
        float distTL = length(vTexCoord - vec2(0.0, 0.0));
        float distTR = length(vTexCoord - vec2(1.0, 0.0));
        float distBL = length(vTexCoord - vec2(0.0, 1.0));
        float distBR = length(vTexCoord - vec2(1.0, 1.0));

        if (!(distTL < uCornerRadius || distTR < uCornerRadius ||
            distBL < uCornerRadius || distBR < uCornerRadius))
        {
            return vec4(0.0, 0.0, 0.0, 0.0);
        }

        return color; 
    }

    vec4 EnableEffects(vec4 color)
    {
        float grey = 0.21 * color.r + 0.71 * color.g + 0.07 * color.b;
        return vec4(grey , grey, grey, 1.0);
    }

    vec4 Red(vec4 color)
    {
        return vec4(color.r * 1.1, color.g * 0.4, color.b * 0.4, 1.0);
    }

    vec4 Green(vec4 color)
    {
        return vec4(color.r * 0.4, color.g * 1.1, color.b * 0.4, 1.0);
    }
    
    vec4 Blue(vec4 color)
    {
        return vec4(color.r * 0.4, color.g * 0.4, color.b * 1.05, 1.0);
    }
    ";

    private const string VertexShader = @"
    #version 330 core

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