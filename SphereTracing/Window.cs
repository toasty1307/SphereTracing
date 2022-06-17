using System.Runtime.InteropServices;
using GLFW;
using SkiaSharp;

namespace SphereTracing;

public class Window
{
    public readonly NativeWindow NativeWindow;
    public readonly SKCanvas Canvas;
    public readonly SKSurface SkiaSurface;

    public Window()
    {
        NativeWindow = new NativeWindow(Program.Width, Program.Height, "toasty made a thing (tried to)");
        var nativeContext = GetNativeContext(NativeWindow);
        var glInterface =
            GRGlInterface.AssembleGlInterface(nativeContext, (_, name) => Glfw.GetProcAddress(name));
        var skiaContext = GRContext.Create(GRBackend.OpenGL, glInterface);
        var framebufferInfo = new GRGlFramebufferInfo(0, GRPixelConfig.Rgba8888.ToGlSizedFormat());
        var backendRenderTarget = new GRBackendRenderTarget(Program.Width, Program.Height, 0, 8, framebufferInfo);
        SkiaSurface =
            SKSurface.Create(skiaContext, backendRenderTarget, GRSurfaceOrigin.BottomLeft, SKImageInfo.PlatformColorType);
        Canvas = SkiaSurface.Canvas;
    }

    private static object GetNativeContext(NativeWindow nativeWindow)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Native.GetWglContext(nativeWindow);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // XServer
            return Native.GetGLXContext(nativeWindow);
            // Wayland
            //return Native.GetEglContext(nativeWindow);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Native.GetNSGLContext(nativeWindow);
        }

        throw new PlatformNotSupportedException();
    }

    public event Action? OnUpdate;

    public void Run()
    {
        while (!NativeWindow.IsClosing)
        {
            OnUpdate?.Invoke();
            Glfw.PollEvents();
        }
    }
}