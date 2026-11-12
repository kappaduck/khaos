// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:property Product="Clear the screen"

using System.Diagnostics;

return KhaosApplication.CreateBuilder(args)
                       .Build<ClearGame>()
                       .Run();

internal sealed class ClearGame() : Game2D(960, 540)
{
    private long _startedAt;

    protected override void Initialize() => _startedAt = Stopwatch.GetTimestamp();

    protected override void Draw(Renderer renderer)
    {
        double seconds = Stopwatch.GetElapsedTime(_startedAt).TotalSeconds;
        float amount = (float)(0.5 + (0.5 * Math.Sin(seconds * Math.PI)));

        renderer.Clear(Color.Lerp(Color.Teal, Color.Crimson, amount));
    }
}
