using System;
using System.Collections.Generic;
using Avalonia.OpenGL;
using static Avalonia.OpenGL.GlConsts;

namespace lab3.Controls.GL;

public class VertexBuffer : OpenGLHelper
{
    private readonly int _vertexArrayObject;
    private readonly int _vertexBufferObject;

    private VertexBuffer(GlInterface GL, int vertexArrayObject, int vertexBufferObject) : base(GL)
    {
        _vertexArrayObject = vertexArrayObject;
        _vertexBufferObject = vertexBufferObject;
    }

    public void Use()
    {
        _gl.BindVertexArray(_vertexArrayObject);
        CheckError();
    }

    public void Destroy()
    {
        _gl.DeleteBuffer(_vertexBufferObject);
        _gl.DeleteVertexArray(_vertexArrayObject);
    }

    private record struct AttributeBinding(int Location, int Size, int StartPosition);

    public class Builder : OpenGLHelper
    {
        private readonly List<AttributeBinding> _attributeBindings;
        private readonly int _stride;
        private readonly float[] _vertices;

        public Builder(GlInterface GL, float[] vertices, int stride) : base(GL)
        {
            _vertices = vertices;
            _stride = stride;
            _attributeBindings = new List<AttributeBinding>();
        }

        public Builder AttributeBinding(int location, int size, int startPosition)
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
            
            return new VertexBuffer(_gl, vertexArrayObject, vertexBufferObject);
        }
        
        private (int, int) Generate()
        {
            var vertexArrayObject = _gl.GenVertexArray();
            var vertexBufferObject = _gl.GenBuffer();

            _gl.BindVertexArray(vertexArrayObject);
            _gl.BindBuffer(GL_ARRAY_BUFFER, vertexBufferObject);
            CheckError();

            return (vertexArrayObject, vertexBufferObject);
        }

        private unsafe void Fill()
        {
            fixed (void* verticesPtr = _vertices)
            {
                _gl.BufferData(
                    GL_ARRAY_BUFFER, new IntPtr(_vertices.Length * sizeof(float)),
                    new IntPtr(verticesPtr), GL_STATIC_DRAW
                );
            }

            CheckError();
        }
        
        private void BindAttributes()
        {
            foreach (var attributeBinding in _attributeBindings)
            {
                _gl.VertexAttribPointer(
                    attributeBinding.Location,
                    attributeBinding.Size,
                    GL_FLOAT,
                    0,
                    _stride,
                    new IntPtr(attributeBinding.StartPosition)
                );
                _gl.EnableVertexAttribArray(attributeBinding.Location);
                CheckError();
            }
        }

        private void Unbind()
        {
            _gl.BindBuffer(GL_ARRAY_BUFFER, 0);
            _gl.BindVertexArray(0);
            CheckError();
        }
    }
}