using ComputeSharp;
// ReSharper disable All

namespace SphereTracing.Shaders;

[AutoConstructor]
public readonly partial struct SphereGameShader : IComputeShader
{
    private const int Width = Program.Width;
    private const int Height = Program.Height;

    public readonly ReadWriteBuffer<uint> buffer;

    [ShaderMethod]
    public static bool OnSphere(Float2 center, float radius, Float2 xy) =>
        Hlsl.Distance(xy, center) <= radius;
    
    public void Execute()
    {
        var pixel = ThreadIds.Y * Width + ThreadIds.X;
        
        var center = new Float2(Width / 2, Height / 2);
        int radius = 100;
        var red = 0;
        var green = 0;
        var blue = 0;
        if (OnSphere(center, radius, ThreadIds.XY))
        {
            red = blue = green = 255;
        }

        buffer[pixel] = (uint)((255 << 24 | blue << 16 | green << 8 | red) & 0xFFFFFFFF);
    }
}
