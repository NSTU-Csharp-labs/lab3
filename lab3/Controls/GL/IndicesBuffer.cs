using System;
using Avalonia.OpenGL;
using static Avalonia.OpenGL.GlConsts;

namespace lab3.Controls.GL;

public class IndicesBuffer : OpenGLHelper
{
    private readonly ushort[] _indices;
    private readonly int _indicesBufferObject;

    public unsafe IndicesBuffer(GlInterface GL, ushort[] indices) : base(GL)
    {
        _indices = indices;
        _indicesBufferObject = _gl.GenBuffer();
        _gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _indicesBufferObject);

        fixed (void* pdata = _indices)
        {
            _gl.BufferData(
                GL_ELEMENT_ARRAY_BUFFER, new IntPtr(_indices.Length * sizeof(ushort)),
                new IntPtr(pdata), GL_STATIC_DRAW
            );
        }

        CheckError();
    }

    public void Use()
    {
        _gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _indicesBufferObject);
    }

    public void Destroy()
    {
        _gl.DeleteBuffer(_indicesBufferObject);
    }
}