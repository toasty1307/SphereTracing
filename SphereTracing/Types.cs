namespace SphereTracing;

public readonly struct Circle
{
    public Circle(int x, int y, int r)
    {
        X = x;
        Y = y;
        R = r;
    }

    public readonly int X;
    public readonly int Y;
    public readonly int R;
}