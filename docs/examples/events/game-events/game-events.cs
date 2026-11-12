// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:property Product="Game events"

using System.Diagnostics;

return KhaosApplication.CreateBuilder(args)
                       .Build<GameEventsGame>()
                       .Run();

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
        switch (e)
        {
            case CustomEvent { GameEvents: LoadingFinished loaded }:
                _levelCount = loaded.Levels;
                break;

            case CustomEvent { GameEvents: LevelFinished finished }:
                _level = (finished.Level + 1) % _levelCount;
                break;

            case KeyPressed { Code: Scancode.Space } when _levelCount > 0:
                _ = EventQueue.TryPush(new LevelFinished(_level));
                break;

            case KeyPressed { Code: Scancode.Escape }:
                Exit();
                break;

            default:
                break;
        }
    }

    private static async Task LoadLevelsAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(2));

        _ = EventQueue.TryPush(new LoadingFinished(_levels.Length));
    }
}
