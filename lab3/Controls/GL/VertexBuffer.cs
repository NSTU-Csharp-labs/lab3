using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace lab3.Controls.GL;

public class VertexBuffer : IDisposable
{
    private readonly VertexArrayHandle _vertexArrayObject;
    private readonly BufferHandle _vertexBufferObject;

    private VertexBuffer(VertexArrayHandle vertexArrayObject, BufferHandle vertexBufferObject)
    {
        _vertexArrayObject = vertexArrayObject;
        _vertexBufferObject = vertexBufferObject;
    }

    public IDisposable Use()
    {
        OpenTK.Graphics.OpenGL.GL.BindVertexArray(_vertexArrayObject);

        return new DisposableUsing(() =>
        {
            OpenTK.Graphics.OpenGL.GL.BindVertexArray(VertexArrayHandle.Zero);
        });
    }

    public void Dispose()
    {
        OpenTK.Graphics.OpenGL.GL.DeleteVertexArray(_vertexArrayObject);
        OpenTK.Graphics.OpenGL.GL.DeleteBuffer(_vertexBufferObject);
    }

    private record struct AttributeBinding(uint Location, int Size, int StartPosition);

    public class Builder
    {
        private readonly List<AttributeBinding> _attributeBindings;
        private readonly int _stride;
        private readonly float[] _vertices;

        public Builder(float[] vertices, int stride)
        {
            _vertices = vertices;
            _stride = stride;
            _attributeBindings = new List<AttributeBinding>();
        }

        public Builder AttributeBinding(uint location, int size, int startPosition)
        {
            _attributeBindings.Add(new AttributeBinding(location, size, startPosition));
            return this;
        }

        public VertexBuffer Build()
        {
            var (vertexArrayObject, vertexBufferObject) = Generate();
            Fill();
            BindAttributes();
            Unbind();

            return new VertexBuffer(vertexArrayObject, vertexBufferObject);
        }

        private (VertexArrayHandle, BufferHandle) Generate()
        {
            var vertexArrayObject = OpenTK.Graphics.OpenGL.GL.GenVertexArray();
            var vertexBufferObject = OpenTK.Graphics.OpenGL.GL.GenBuffer();

            OpenTK.Graphics.OpenGL.GL.BindVertexArray(vertexArrayObject);
            OpenTK.Graphics.OpenGL.GL.BindBuffer(BufferTargetARB.ArrayBuffer, vertexBufferObject);
            OpenGlUtils.CheckError();

            return (vertexArrayObject, vertexBufferObject);
        }

        private void Fill()
        {
            OpenTK.Graphics.OpenGL.GL.BufferData(
                BufferTargetARB.ArrayBuffer,
                new ReadOnlySpan<float>(_vertices),
                BufferUsageARB.DynamicDraw
            );

            OpenGlUtils.CheckError();
        }

        private void BindAttributes()
        {
            foreach (var attributeBinding in _attributeBindings)
            {
                OpenTK.Graphics.OpenGL.GL.VertexAttribPointer(
                    attributeBinding.Location,
                    attributeBinding.Size,
                    VertexAttribPointerType.Float,
                    false,
                    _stride * sizeof(float),
                    attributeBinding.StartPosition * sizeof(float)
                );

                OpenTK.Graphics.OpenGL.GL.EnableVertexAttribArray(attributeBinding.Location);
                OpenGlUtils.CheckError();
            }
        }

        private void Unbind()
        {
            OpenTK.Graphics.OpenGL.GL.BindBuffer(BufferTargetARB.ArrayBuffer, BufferHandle.Zero);
            OpenTK.Graphics.OpenGL.GL.BindVertexArray(VertexArrayHandle.Zero);
        }
    }
}