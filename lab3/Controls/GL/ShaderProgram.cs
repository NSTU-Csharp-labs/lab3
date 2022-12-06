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
    
   

    public ShaderProgram(GlInterface GL, GlProfileType type)
        : base(GL)
    {
        _type = type;
        _vertexShader = AddShader( GL_VERTEX_SHADER, false, VertexShader);

        _fragmentShader = AddShader(GL_FRAGMENT_SHADER, true, FragmentShader);
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

    public unsafe void SetUniformMatrix4X4(string name, Matrix4x4 matrix)
    {
        _gl.UniformMatrix4fv(_gl.GetUniformLocationString(_link, name), 1, false, &matrix);
        CheckError();
    }
    
    public void SetUniformBool(string name, bool flag)
    {
        CheckError();
        _gl.Uniform1f(_gl.GetUniformLocationString(_link, name), flag switch
        {
            true => 1f,
            false => 0f
        });
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
    
    private const string FragmentShader = @"
    varying vec2 vTexCoord;
    uniform sampler2D uTexture;
    
    uniform bool uBlackAndWhite;
    uniform bool uRed;
    uniform bool uBlue;
    uniform bool uGreen;

    vec4 EnableEffects(vec4 color);
    vec4 Green(vec4 color);
    vec4 Red(vec4 color);
    vec4 Blue(vec4 color);
    

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
        gl_FragColor = color;   
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