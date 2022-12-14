using System;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace lab3.Controls.GL;

public class IndicesBuffer : IDisposable
{
    private readonly ushort[] _indices;
    private readonly BufferHandle _indicesBufferObject;

    public IndicesBuffer(ushort[] indices)
    {
        _indices = indices;
        _indicesBufferObject = OpenTK.Graphics.OpenGL.GL.GenBuffer();

        Fill();
    }

    public void Dispose()
    {
        OpenTK.Graphics.OpenGL.GL.DeleteBuffer(_indicesBufferObject);
    }

    private void Fill()
    {
        OpenTK.Graphics.OpenGL.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer,
            _indicesBufferObject);

        OpenTK.Graphics.OpenGL.GL.BufferData(
            BufferTargetARB.ElementArrayBuffer,
            new ReadOnlySpan<ushort>(_indices),
            BufferUsageARB.DynamicDraw
        );

        OpenTK.Graphics.OpenGL.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer,
            BufferHandle.Zero);

        OpenGlUtils.CheckError();
    }

    public IDisposable Use()
    {
        OpenTK.Graphics.OpenGL.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer,
            _indicesBufferObject);

        return new DisposableUsing(() =>
        {
            OpenTK.Graphics.OpenGL.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer,
                BufferHandle.Zero);
        });
    }
}