// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Graphics.Rendering;
using KappaDuck.Khaos.Hosting;
using KappaDuck.Khaos.Windowing;
using Microsoft.Extensions.Logging.Abstractions;

namespace KappaDuck.Khaos.Benchmarks.Frame;

public class FrameBenchmarks
{
    private EngineHost _host = null!;
    private Window _window = null!;
    private Renderer _renderer = null!;

    private int _frame;

    [GlobalSetup]
    public void StartEngine()
    {
        Headless.UseOffscreen();

        _host = EngineHost.Start(new AppMetadata() { Name = "Benchmarks" });
        _window = Window.Open(new WindowOptions() { Title = "Benchmarks", Width = 1280, Height = 720 });
        _renderer = Renderer.Create(_window, VSync.Disabled, NullLogger<Renderer>.Instance);
    }

    [GlobalCleanup]
    public void StopEngine()
    {
        _renderer.Destroy();
        _window.Close();
        _host.Dispose();
    }

    [Benchmark(Baseline = true)]
    public void EmptyFrame()
    {
        _renderer.Clear(Color.Black);
        _renderer.Present();
    }

    [Benchmark]
    public void FrameWithTitleCounter()
    {
        _renderer.Clear(Color.Black);
        _window.SetTitle($"Benchmarks - frame {_frame++}");
        _renderer.Present();
    }

    [Benchmark]
    public void FrameWithTitleString()
    {
        _renderer.Clear(Color.Black);
        _window.Title = "Benchmarks - frame " + _frame++;
        _renderer.Present();
    }
}
