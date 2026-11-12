// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:property Product="Clear the screen"

return KhaosApplication.CreateBuilder(args)
                       .Build<ClearGame>()
                       .Run();

internal sealed class ClearGame() : Game2D(960, 540)
{
    private Color _color = Color.Teal;

    protected override void Draw(Renderer renderer, in Frame frame) => renderer.Clear(_color);

    protected override void Update(in GameTime time)
    {
        float amount = (float)(0.5 + (0.5 * Math.Sin(time.Total * Math.PI)));
        _color = Color.Lerp(Color.Teal, Color.Crimson, amount);
    }
}
