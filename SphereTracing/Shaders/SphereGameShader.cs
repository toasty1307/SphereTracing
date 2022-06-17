using ComputeSharp;
using SkiaSharp;

// ReSharper disable All

namespace SphereTracing.Shaders;

[AutoConstructor]
public readonly partial struct SphereGameShader : IComputeShader
{
    private const int Width = Program.Width;
    private const int Height = Program.Height;

    public readonly ReadWriteBuffer<SKColor> buffer;
    public readonly ReadOnlyBuffer<Circle> circleBuffer;
    public readonly ReadOnlyBuffer<float> positionAndDirectionBuffer;

    [ShaderMethod]
    public static bool OnCircle(Float2 center, float radius, Float2 xy) => 
        CircleDistance(center, radius, xy) <= 0;
    
    [ShaderMethod]
    public static float CircleDistance(Float2 center, float radius, Float2 xy) =>
        Hlsl.Distance(xy, center) - radius;
    
    [ShaderMethod]
    public static bool OnLine(Float2 start, Float2 end, float width, Float2 xy) =>
        Hlsl.Distance(xy, start) + Hlsl.Distance(xy, end) - Hlsl.Distance(start, end) <= width;

    public float ClosestShapeDistance(Float2 xy)
    {
        float closestDistance = float.MaxValue;
        for (var index = 0; index < circleBuffer.Length; index++)
        {
            var circle = circleBuffer[index];
            var distance = CircleDistance(new Float2(circle.X, circle.Y), circle.R, xy);
            if (distance < closestDistance)
                closestDistance = distance;
        }

        return closestDistance;
    }

    public void Execute()
    {
        var pixel = ThreadIds.Y * Width + ThreadIds.X;
        
        int red = 0, green = 0, blue = 0;
        // spheres
        for (int i = 0; i < circleBuffer.Length; i++)
        {
            var circle = circleBuffer[i];
            if (OnCircle(new Float2(circle.X, circle.Y), circle.R, ThreadIds.XY))
            {
                red = green = blue = 255;
                break;
            }
        }

        var currentPos = new Float2(positionAndDirectionBuffer[0], positionAndDirectionBuffer[1]);

        var target = new Float2(positionAndDirectionBuffer[2], positionAndDirectionBuffer[3]);
        
        /*
        if (OnLine(currentPos, target, .01f, ThreadIds.XY))
        {
            blue = 255;
            red = green = 0;
        }
        */
        
        int maxIterations = 50;

        var closestDistance = ClosestShapeDistance(currentPos);
        var direction = Hlsl.Normalize(target - currentPos);
        var posToPlaceCircle = currentPos;
        while (maxIterations > 0 && closestDistance > 2)
        {
            
            if (OnCircle(posToPlaceCircle, closestDistance, ThreadIds.XY) && !OnCircle(posToPlaceCircle, closestDistance - 2, ThreadIds.XY))
            {
                red = 120;
                green = 120;
                blue = 120;
            }

            /*
            if (OnCircle(posToPlaceCircle, closestDistance - 2, ThreadIds.XY))
            {
                red = 0;
                green = 0;
                blue = 0;
            }
            */

            posToPlaceCircle = posToPlaceCircle + direction * closestDistance;
            closestDistance = ClosestShapeDistance(posToPlaceCircle);

            maxIterations--;
        }
        
        if (OnCircle(currentPos, 10, ThreadIds.XY))
        {
            red = 255;
            green = blue = 0;
        }

        if (OnCircle(target, 10, ThreadIds.XY))
        {
            green = 255;
            red = blue = 0;
        }

        if (red != 0 || green != 0 || blue != 0)
        {
            buffer[pixel] = new SKColor((uint)((255 << 24 | (red & 0xFF) << 16 | (green & 0xFF) << 8 | (blue & 0xFF)) & 0xFFFFFFFF));
        }
    }
}
