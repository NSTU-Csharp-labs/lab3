using System;

namespace lab3.Controls.GL;

public class DisposableUsing : IDisposable
{
    private readonly Action _action;

    public DisposableUsing(Action action) => _action = action;

    public void Dispose() => _action.Invoke();
}