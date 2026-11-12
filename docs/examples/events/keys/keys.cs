// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:property Product="Keys"

return KhaosApplication.CreateBuilder(args)
                       .Build<KeysGame>()
                       .Run();

internal sealed class KeysGame() : Game2D(960, 540)
{
    private Color _color = Color.Teal;

    protected override void Draw(Renderer renderer) => renderer.Clear(_color);

    protected override void OnEvent(in Event e)
    {
        switch (e)
        {
            case KeyPressed { Code: Scancode.R }:
                _color = Color.Crimson;
                break;

            case KeyPressed { Code: Scancode.G }:
                _color = Color.SeaGreen;
                break;

            case KeyPressed { Code: Scancode.B }:
                _color = Color.RoyalBlue;
                break;

            case KeyReleased { Code: Scancode.R or Scancode.G or Scancode.B }:
                _color = Color.Teal;
                break;

            case KeyPressed { Code: Scancode.Escape }:
                Exit();
                break;

            default:
                break;
        }
    }
}
