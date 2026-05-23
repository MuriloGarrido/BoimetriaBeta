namespace BoimetriaBeta.Models;

public readonly record struct BoundingBox(float X1, float Y1, float X2, float Y2)
{
    public float Width => X2 - X1;
    public float Height => Y2 - Y1;
}
