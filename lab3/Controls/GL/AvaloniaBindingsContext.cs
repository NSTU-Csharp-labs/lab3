using System;
using Avalonia.OpenGL;
using OpenTK;

namespace lab3.Controls.GL;

public class AvaloniaBindingsContext : IBindingsContext
{
    private readonly GlInterface _glInterface;

    public AvaloniaBindingsContext(GlInterface glInterface)
    {
        _glInterface = glInterface;
    }

    public IntPtr GetProcAddress(string procName)
    {
        return _glInterface.GetProcAddress(procName);
    }
}