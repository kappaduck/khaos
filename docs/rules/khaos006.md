# KHAOS006: a game takes its window as a constructor parameter

| Item     | Value          |
| :------- | :------------- |
| Category | Hosting        |
| Severity | Warning        |
| Where    | Your game code |

## Cause

The constructor of your game, a class that derives from `Game2D`, has a parameter of type `Window`.

## Why

Your game already has its window: `Game2D` gives it as the `Window` property. The parameter receives the same window,
so it is a second way to reach it. It is also a trap, because the window opens after your game is created: using it
in the constructor throws, and works only from `Initialize` on.

A service that needs the window, such as a camera, can still take it in its constructor. Only a game is reported.

## Reported

```csharp
public sealed class PinballGame(Window window) : Game2D(800, 600)
//                                     ~~~~~~ KHAOS006: 'PinballGame' takes the window as 'window', but Game2D
//                                            already gives it as Window; use Window from Initialize on
{
    protected override void Initialize() => window.Title = "Pinball";
}
```

## Fix

Remove the parameter and use `Window`:

```csharp
public sealed class PinballGame() : Game2D(800, 600)
{
    protected override void Initialize() => Window.Title = "Pinball";
}
```

## Suppressing

There is no reason to: `Window` is always the same window as the parameter.
