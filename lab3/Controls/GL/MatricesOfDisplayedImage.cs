using System.Numerics;

namespace lab3.Controls.GL;

public class MatricesOfDisplayedImage : IMatricesProvider
{
    private readonly Matrix4x4 _reflectionMatrix = Matrix4x4.CreateReflection
    (
        new Plane(0, 1, 0, 0)
    );

    private readonly Vector3 _cameraUp = new(0, 1, 0);
    
    private readonly AdjustedBitmap _bitmap;
    
    public MatricesOfDisplayedImage(AdjustedBitmap bitmap)
    {
        _bitmap = bitmap;
    }

    public Matrix4x4 Projection =>
        Matrix4x4.CreateOrthographicOffCenter
        (
            -(float)_bitmap.BoundsWidth / 2,
            (float)_bitmap.BoundsWidth / 2,
            -(float)_bitmap.BoundsHeight / 2,
            (float)_bitmap.BoundsHeight / 2,
            0,
            10
        );

    public Matrix4x4 View => Matrix4x4.CreateLookAt
    (
        new Vector3(_bitmap.AdjustedWidth / 2, _bitmap.AdjustedHeight / 2, 1),
        new Vector3(_bitmap.AdjustedWidth / 2, _bitmap.AdjustedHeight / 2, 0),
        _cameraUp
    );

    public Matrix4x4 Model => Matrix4x4.Multiply
    (
        Matrix4x4.CreateScale
        (
            _bitmap.AdjustedWidth,
            _bitmap.AdjustedHeight,
            1
        ),
        _reflectionMatrix
    );
}