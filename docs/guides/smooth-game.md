# Keep your game smooth

At 60 frames per second, your game has 16 milliseconds to draw each frame. This page explains what Khaos does to keep
that pace, what to watch in your own code, and how to check your game.

> [!NOTE]
> Khaos is young: the names on this page can still change from one version to the next until 1.0.

## Memory and stutters

.NET frees the memory your game no longer uses with its **garbage collector**. A collection pauses the game for a
moment. In a menu, nobody notices. In the middle of a jump, the player sees a stutter.

A collection only happens after the game has asked for enough new memory, which is called **allocating**. Code that
runs every frame and allocates nothing never causes one.

## What Khaos promises

Once the first frames have run, the game loop allocates nothing: handling events, updating and drawing. Tests check
it on every change to Khaos.

`Initialize()` can allocate: it runs once, before the first frame, so a collection there happens before the player is
playing.

## Keep your frames free

Your `OnEvent`, `FixedUpdate`, `Update` and `Draw` run every frame too. These are the usual reasons a frame
allocates:

| Allocates every time it runs                     | Instead                                                       |
| :----------------------------------------------- | :------------------------------------------------------------ |
| `new` on a class, an array or a `List<T>`        | Create it once in `Initialize()` and reuse it, with `Clear()` |
| Building a string, such as `$"Score: {score}"`   | Build it again only when the score changes                    |
| LINQ: `Where`, `Select`, `OrderBy`               | A `for` or `foreach` loop                                     |
| A lambda that uses a local variable              | A method, or a `static` lambda that uses none                 |
| `foreach` over an `IEnumerable<T>` or `IList<T>` | `foreach` over the `List<T>` or the array itself              |

A `struct` such as `Vector2` or `Color` never allocates on its own: create as many as you need.

## Check your game

The garbage collector counts its collections in `GC.CollectionCount(0)`. Write the count each time it changes, then
play your game for a few minutes:

```csharp
internal sealed class MyGame() : Game2D("Ball", 1280, 720)
{
    private int _collections;

    protected override void Initialize()
    {
        // Load your images, sounds and fonts first.
        _collections = GC.CollectionCount(0);
    }

    protected override void Update(in GameTime time)
    {
        int collections = GC.CollectionCount(0);

        if (collections == _collections)
            return;

        _collections = collections;
        Console.WriteLine($"Garbage collections: {collections}");
    }
}
```

On the desktop the line shows in the terminal, and in the browser in the **Console** tab of the developer tools
(<kbd>F12</kbd>).

If nothing is written while you play, your frames allocate nothing. If a line comes back every few seconds, something
in your frames allocates: look at the table above. Play for a few minutes rather than seconds, since a frame that
allocates a little can take a minute to fill the memory a collection waits for.

### Check a published build too

While you develop, .NET optimizes your code as it runs, and it can remove allocations that your published game still
makes. Check a published build before you call it done. A Staging build is a published build with your logs and the
overlay still inside:

```bash
dotnet publish MyGame.Desktop -c Release -p:KhaosEnvironment=Staging
dotnet publish MyGame.Web -c Release -p:KhaosEnvironment=Staging
```

See [Configuration and environments](configuration.md#choosing-the-environment).

### Live numbers

To follow memory live without changing your code, the .NET tool
[dotnet-counters](https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-counters) shows the collections and the
memory allocated while the game runs on the desktop. A published desktop game needs this line in its `.csproj` for
the tool to see it:

```xml
<EventSourceSupport>true</EventSourceSupport>
```

## Frame rate

To see how fast your game draws where players will play it, open the overlay with <kbd>F1</kbd> in a Staging build.
See [Configuration and environments](configuration.md).
