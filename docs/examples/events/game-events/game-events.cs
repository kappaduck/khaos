// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:property Product="Game events"

using KappaDuck.Khaos;
using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Graphics.Rendering;
using KappaDuck.Khaos.Input;
using System.Diagnostics;

return new GameEventsGame().Run();

internal readonly record struct LoadingFinished(int Levels);

internal readonly record struct LevelFinished(int Level);

[CustomEvents(typeof(LoadingFinished), typeof(LevelFinished))]
internal readonly partial struct GameEvents;

internal sealed class GameEventsGame() : Game2D(960, 540)
{
    private static readonly Color[] _levels = [Color.Teal, Color.Gold, Color.Crimson];

    private int _level;
    private int _levelCount;
    private long _startedAt;

    protected override void Draw(Renderer renderer)
    {
        if (_levelCount == 0)
        {
            double seconds = Stopwatch.GetElapsedTime(_startedAt).TotalSeconds;
            float amount = (float)(0.5 + (0.5 * Math.Sin(seconds * Math.PI * 2)));

            renderer.Clear(Color.Lerp(Color.DimGray, Color.Silver, amount));
            return;
        }

        renderer.Clear(_levels[_level]);
    }

    protected override void Initialize()
    {
        _startedAt = Stopwatch.GetTimestamp();
        _ = Task.Run(LoadLevelsAsync);
    }

    protected override void OnEvent(in Event e)
    {
        if (e is CustomEvent { GameEvents: LoadingFinished loaded })
        {
            Console.WriteLine("Yeah!");
            _levelCount = loaded.Levels;
            return;
        }

        if (e is CustomEvent { GameEvents: LevelFinished finished })
        {
            _level = (finished.Level + 1) % _levelCount;
            return;
        }

        if (e is KeyPressed { Code: Scancode.Space } && _levelCount > 0)
        {
            _ = EventQueue.TryPush(new LevelFinished(_level));
            return;
        }

        if (e is KeyPressed { Code: Scancode.Escape })
            Exit();
    }

    private static async Task LoadLevelsAsync()
    {
        Console.WriteLine(":D");
        await Task.Delay(TimeSpan.FromSeconds(2));

        Console.WriteLine("Push it!");
        _ = EventQueue.TryPush(new LoadingFinished(_levels.Length));
    }
}
