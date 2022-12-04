using System;
using Avalonia.OpenGL;
using static Avalonia.OpenGL.GlConsts;

namespace lab3.Controls.GL;

public class VertexBuffer : OpenGLHelper
{
    private int _vertexBufferObject;
    private int _vertexArrayObject;
    private int _stride;
    private readonly float[] _vertices;
    
    public VertexBuffer(GlInterface GL, float[] vertices, int stride)
    {
        _gl = GL;
        _vertexBufferObject = _gl.GenBuffer();
        _gl.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
        _vertexArrayObject = _gl.GenVertexArray();
      //  _gl.BindBuffer(GL_ARRAY_BUFFER, 0);
        _gl.BindVertexArray(_vertexArrayObject);
        _vertices = vertices;
        _stride = stride;
        
        Fill();
    }

    private unsafe void Fill()
    {
        _gl.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
        
        fixed (void* pdata = _vertices)
            _gl.BufferData(
                GL_ARRAY_BUFFER, new IntPtr(_vertices.Length * sizeof(float)),
                new IntPtr(pdata), GL_STATIC_DRAW
            );
        
        _gl.BindBuffer(GL_ARRAY_BUFFER, 0);
    }

    public void BindAttribute(int location, int size, int startPosition)
    {
        Bind();
        _gl.VertexAttribPointer(location, size, GL_FLOAT, 0, _stride * sizeof(float), new IntPtr(startPosition * sizeof(float)));
        _gl.EnableVertexAttribArray(location);
    }

    public void Bind()
    {
        _gl.BindVertexArray(_vertexArrayObject);
        _gl.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
        CheckError();
    }

    public void Unbind()
    {
        _gl.BindBuffer(GL_ARRAY_BUFFER, 0);
        _gl.BindVertexArray(0);
        CheckError();
    }

    public void Destroy()
    {
        Unbind();
        _gl.DeleteBuffer(_vertexBufferObject);
        _gl.DeleteVertexArray(_vertexArrayObject);
    }
}