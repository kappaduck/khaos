// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:property Product="Keys"

using KappaDuck.Khaos;
using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Graphics.Rendering;
using KappaDuck.Khaos.Input;

return new KeysGame().Run();

internal sealed class KeysGame() : Game2D(960, 540)
{
    private Color _color = Color.Teal;

    protected override void Draw(Renderer renderer) => renderer.Clear(_color);

    protected override void OnEvent(in Event e)
    {
        if (e is KeyPressed { Code: Scancode.R })
        {
            _color = Color.Crimson;
            return;
        }

        if (e is KeyPressed { Code: Scancode.G })
        {
            _color = Color.SeaGreen;
            return;
        }

        if (e is KeyPressed { Code: Scancode.B })
        {
            _color = Color.RoyalBlue;
            return;
        }

        if (e is KeyReleased { Code: Scancode.R or Scancode.G or Scancode.B })
        {
            _color = Color.Teal;
            return;
        }

        if (e is KeyPressed { Code: Scancode.Escape })
            Exit();
    }
}
