# Demo

Games made with Khaos, to show what the engine can do: small ones that each show a few features, up to a full
game someone would play. Each one is the shape `dotnet new khaos` creates:

```text
demo/<Name>/
├─ <Name>/            the game: code, configuration, assets/
├─ <Name>.Desktop/    the desktop app: Program.cs and a reference to the game
└─ <Name>.Web/        the browser app: the same, on Microsoft.NET.Sdk.WebAssembly
```

| Demo                           | Shows                               |
| :----------------------------- | :---------------------------------- |
| [Breakout](Breakout/readme.md) | Sprites, collisions, sound, a score |

To run a demo:

```bash
dotnet run --project demo/<Name>/<Name>.Desktop
dotnet run --project demo/<Name>/<Name>.Web
```
