# SphereTracing — Ray Marching with Signed Distance Fields

A real-time **ray marching** renderer in C# using **Signed Distance Functions (SDFs)** and OpenGL compute shaders.

Ray marching (also called sphere tracing) is an alternative to traditional rasterization or ray-triangle intersection. Instead of testing rays against geometry explicitly, the scene is described by mathematical distance functions — enabling rendering of perfectly smooth surfaces, fractals, and complex implicit geometry that would be impossible to represent as meshes.

## Features

- SDF-based scene description (spheres, boxes, smooth blending)
- `smoothmin` operator for organic shape blending
- Phong shading with soft shadows via secondary ray marching
- Real-time rendering with OpenGL + GLFW

## How It Works

Each pixel fires a ray from the camera. Rather than checking triangle intersections, the ray **marches forward in steps** — each step sized by the minimum SDF value across all scene objects (guaranteed safe step distance). When the ray gets close enough to a surface, it shades the point.

```
for each step:
    d = min(sdf_sphere(p), sdf_box(p), ...)
    if d < epsilon: HIT → shade
    p += ray_dir * d
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- OpenGL 4.3+ capable GPU
- `libglfw` installed:
  ```sh
  # Ubuntu/Debian
  sudo apt install libglfw3

  # Arch
  sudo pacman -S glfw
  ```

### Run

```sh
git clone https://github.com/gqvz/SphereTracing
cd SphereTracing
dotnet run --project SphereTracing
```

> If you get a GLFW library not found error, check the exact `.so` name on your system (`ldconfig -p | grep glfw`) and update the dllmap in the project accordingly.

## References

- [Inigo Quilez — SDF functions](https://iquilezles.org/articles/distfunctions/)
- [Jamie Wong — Ray Marching and SDFs](https://jamie-wong.com/2016/07/15/ray-marching-signed-distance-functions/)
