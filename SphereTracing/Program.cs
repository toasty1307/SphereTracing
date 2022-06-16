using System.Diagnostics;
using System.Runtime.CompilerServices;
using ComputeSharp;
using SixLabors.ImageSharp;
using SphereTracing.Shaders;
using Rgba32 = SixLabors.ImageSharp.PixelFormats.Rgba32;

namespace SphereTracing
{
    public class Program
    {
        public const int Width = 1920;
        public const int Height = 1080;

        private static void Main()
        {
            Console.WriteLine("Starting...");
            var sw = Stopwatch.StartNew();
            
            var array = new uint[Width * Height];

            using var buffer = GraphicsDevice.Default.AllocateReadWriteBuffer(array);
            
            GraphicsDevice.Default.For(Width, Height, new SphereGameShader(buffer));

            buffer.CopyTo(array);
            
            sw.Stop();
            Console.WriteLine($"Done! The render took {sw.Elapsed.TotalSeconds} seconds.\nSaving Image...");
            
            var image = new Image<Rgba32>(Width, Height);
            var pixels = Unsafe.As<uint[], Rgba32[]>(ref array);
            image.Frames.AddFrame(pixels);
            image.Frames.RemoveFrame(0);
            image.SaveAsPng("output.png");
            
            Console.WriteLine("Saved Image! Press any key to open image and exit.");
            Console.ReadKey();
            
            Process.Start(new ProcessStartInfo("output.png"){UseShellExecute = true});
        }
    }
}