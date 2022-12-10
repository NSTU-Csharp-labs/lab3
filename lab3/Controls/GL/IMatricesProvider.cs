using System.Numerics;

namespace lab3.Controls.GL;

public interface IMatricesProvider
{
    public Matrix4x4 Model { get; }    
    
    public Matrix4x4 View { get; }
    
    public Matrix4x4 Projection { get; }
}