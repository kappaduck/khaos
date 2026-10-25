# Examples

Short programs that each show one thing: clearing the screen, drawing a sprite, reading the keyboard, playing a
sound. Every example runs right on its page, in your browser, with all of its code below it.

They are made to be copied. Find the feature you need, read the few lines that do it, and bring them into your game.

## How to use them

- **Play it on the page**, to see what the example does.
- **Read the code below it.** It is the complete program, usually a few dozen lines, with nothing hidden.
- **Run it yourself** if you want to change things and see what happens. Each example is a single C# file that
  .NET runs directly. From a clone of the [Khaos repository](https://github.com/kappaduck/khaos):

  ```bash
  dotnet docs/examples/rendering/clear/clear.cs                           # on your computer
  dotnet run --project docs/examples/Web -p:Example=rendering/clear       # in your browser
  ```

  Replace `rendering/clear` with the category and name of the example, as shown in its address.

## What is coming

Examples arrive with the features they show: a feature is finished when its example runs in the browser.

| Category  | Examples                                               |
| :-------- | :----------------------------------------------------- |
| Rendering | Clear the screen, a sprite, a sprite sheet             |
| Input     | Keyboard, mouse, gamepad, input actions                |
| Audio     | A sound, music, volume groups                          |
| Text      | Text on screen, text that wraps                        |
| Assets    | The asset catalog                                      |
| Toolkit   | Scenes, camera, tilemap, particles, tweens, collisions |

The [roadmap](../guides/roadmap.md) says when each feature lands.
