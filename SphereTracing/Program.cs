using System.Diagnostics;
using ComputeSharp;
using GLFW;
using SkiaSharp;
using SphereTracing.Shaders;

namespace SphereTracing
{
    public class Program
    {
        private static Window _window = null!;
        public const int Width = 1600;
        public const int Height = 900;

        private static readonly Circle[] Circles = CreateCircles();
        private static readonly Stopwatch Stopwatch = new();

        private static void Main()
        {
            _window = new Window();
            Glfw.SetWindowAttribute(_window.NativeWindow.Handle, WindowAttribute.Resizable, false);
            _window.NativeWindow.CursorMode = CursorMode.Hidden;
            _window.NativeWindow.KeyAction += OnKeyAction;
            Glfw.SetCursorPositionCallback(_window.NativeWindow, MouseCallback);
            _window.OnUpdate += WindowOnOnUpdate;
            Stopwatch.Start();
            _window.Run();
        }

        private static void MouseCallback(IntPtr window, double x, double y)
        {
            _targetX = (float)x;
            _targetY = (float)y;

            _positionAndDirectionBuffer.Dispose();
            _positionAndDirectionBuffer = GraphicsDevice.Default.AllocateReadOnlyBuffer(new []{_x, _y, _targetX, _targetY});
        }

        private static float _x = 50;
        private static float _y = 50;
        private static float _xToChange;
        private static float _yToChange;
        private static float _targetX;
        private static float _targetY;
        
        private static void OnKeyAction(object? sender, KeyEventArgs e)
        {
            const int speed = 3;
            if (e.Key == Keys.A)
                _xToChange += e.State is InputState.Press or InputState.Repeat ? -speed : -_xToChange;
            if (e.Key == Keys.D)
                _xToChange += e.State is InputState.Press or InputState.Repeat ? +speed : -_xToChange;
            if (e.Key == Keys.W)
                _yToChange += e.State is InputState.Press or InputState.Repeat ? -speed : -_yToChange;
            if (e.Key == Keys.S) 
                _yToChange += e.State is InputState.Press or InputState.Repeat ? +speed : -_yToChange;
            _xToChange = Math.Clamp(_xToChange, -speed, speed);
            _yToChange = Math.Clamp(_yToChange, -speed, speed);
        }

        private static readonly SKPaint FpsPaint = new() {Color = SKColor.Parse("#F34336"), TextSize = 18, IsAntialias = true};
        private static readonly ReadOnlyBuffer<Circle> CirclesBuffer = GraphicsDevice.Default.AllocateReadOnlyBuffer(Circles);
        private static ReadWriteBuffer<SKColor> _pixelsBuffer = null!;
        private static readonly SKBitmap Bitmap = new(Width, Height);
        private static ReadOnlyBuffer<float> _positionAndDirectionBuffer = GraphicsDevice.Default.AllocateReadOnlyBuffer(new float[]{50, 50, 100, 100});
        private static unsafe void WindowOnOnUpdate()
        {
            if (_xToChange is not 0 || _yToChange is not 0)
            {
                _x += _xToChange;
                _y += _yToChange;
                _x = Math.Clamp(_x, 10, Width - 10);
                _y = Math.Clamp(_y, 10, Height - 10);
                _positionAndDirectionBuffer.Dispose();
                _positionAndDirectionBuffer = GraphicsDevice.Default.AllocateReadOnlyBuffer(new []{_x, _y, _targetX, _targetY});
            }

            Stopwatch.Stop();
            var elapsed = Stopwatch.Elapsed.TotalMilliseconds;
            if (elapsed < 16.6666666666667D)
            {
                Thread.Sleep((int) (16.6666666666667D - elapsed));
            }
            Stopwatch.Restart();

            _window.Canvas.Clear(SKColor.Parse("#282C34"));
            _pixelsBuffer?.Dispose();
            _pixelsBuffer = GraphicsDevice.Default.AllocateReadWriteBuffer<SKColor>(Height * Width, AllocationMode.Clear);
            GraphicsDevice.Default.For(Width, Height, new SphereGameShader(_pixelsBuffer, CirclesBuffer, _positionAndDirectionBuffer));
            fixed (SKColor* ptr = &_pixelsBuffer.ToArray()[0])
                Bitmap.SetPixels((IntPtr)ptr);
            _window.Canvas.DrawBitmap(Bitmap, 0, 0);
            _window.Canvas.DrawText($"FPS: {1000 / elapsed:N0}", new SKPoint(10, 20), FpsPaint);
            _window.Canvas.Flush();
            _window.NativeWindow.SwapBuffers();
        }

        private static Circle[] CreateCircles()
        {
            const int count = 15;
            var circles = new Circle[count];
            for (var i = 0; i < count; i++)
            {
                var radius = Random.Shared.Next(10, 100);
                var x = Random.Shared.Next(radius, Width - radius);
                var y = Random.Shared.Next(radius, Height - radius);
                circles[i] = new Circle(x, y, radius);
            }

            return circles;
        }
    }
}