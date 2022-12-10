using System.Numerics;

namespace lab3.Controls.GL;

public class OnlyPostprocessingMatrices : IMatricesProvider
{
    public Matrix4x4 Model => Matrix4x4.Identity;
    public Matrix4x4 View => Matrix4x4.Identity;
    public Matrix4x4 Projection => Matrix4x4.Identity;
}