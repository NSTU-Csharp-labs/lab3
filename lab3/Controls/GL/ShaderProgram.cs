using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia.OpenGL;
using static Avalonia.OpenGL.GlConsts;

namespace lab3.Controls.GL;

public class ShaderProgram : OpenGLHelper
{
    private readonly int _fragmentShader;
    private readonly int _link;
    private readonly GlProfileType _type;

    private readonly int _vertexShader;

    private int _lastAttributeIndex;

    public ShaderProgram(GlInterface GL, GlProfileType type, string vertexShaderSource, string fragmentShaderSource)
        : base(GL)
    {
        _type = type;
        _vertexShader = AddShader(GL_VERTEX_SHADER, false, vertexShaderSource);
        _fragmentShader = AddShader(GL_FRAGMENT_SHADER, true, fragmentShaderSource);
        _link = _gl.CreateProgram();
        _lastAttributeIndex = 0;
    }

    private int AddShader(int shaderType, bool fragment, string shaderCode)
    {
        var shader = _gl.CreateShader(shaderType);
        var returned = _gl.CompileShaderAndGetError(shader, GetShader(fragment, shaderCode));
        if (!CheckOpenGlException(returned))
            throw new OpenGlException(
                $"OpenGl: add shader error. {returned} is fragment: {fragment.ToString()} ");
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
        _gl.AttachShader(_link, _vertexShader);
        _gl.AttachShader(_link, _fragmentShader);
        CheckError();
    }

    public void Use()
    {
        _gl.UseProgram(_link);
    }

    public unsafe void SetUniformMatrix4x4(string name, Matrix4x4 matrix)
    {
        _gl.UniformMatrix4fv(_gl.GetUniformLocationString(_link, name), 1, false, &matrix);
        CheckError();
    }


    public int GetAttribLocation(string name)
    {
        _gl.BindAttribLocationString(_link, _lastAttributeIndex, name);
        CheckError();
        
        var index = _lastAttributeIndex;
        _lastAttributeIndex += 1;
        
        return index;
    }

    private void LinkShaderProgram()
    {
        var returned = _gl.LinkProgramAndGetError(_link);
        if (!CheckOpenGlException(returned)) throw new OpenGlException("OpenGl: link shader program error");

        CheckError();
    }

    private bool CheckOpenGlException(string? returned)
    {
        return returned == null || returned.Length.Equals(0);
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

    public void Destroy()
    {
        _gl.DeleteProgram(_link);

        _gl.DeleteShader(_vertexShader);
        _gl.DeleteShader(_fragmentShader);
    }
}