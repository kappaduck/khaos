# Positions, sizes and angles

A 2D game is full of numbers that belong together. Where the player stands, how big a sprite is, which way a ship
faces. Khaos gives each of them a small type in `KappaDuck.Khaos.Mathematics`, and uses `Vector2` from .NET for
movement. This page shows which one to pick and the few rules that surprise people.

> [!NOTE]
> Khaos is young: the names on this page can still change from one version to the next until 1.0.

## The screen

The point (0, 0) is the top-left corner of the screen. X grows to the right and **Y grows downward**, so a point at
(100, 50) is 100 pixels from the left edge and 50 from the top. To move something up, make its Y smaller.

Everything in Khaos follows that rule. `Vector2.Up` is (0, -1), `Vector2.Down` is (0, 1), and a positive angle turns
clockwise on the screen.

> [!TIP]
> Coming from Unity? Its world points Y up, and its `Vector2.up` is (0, 1)
> ([Unity docs](https://docs.unity.com/en-us/engine/current/script-reference/unityengine/vector2/up)). Godot's 2D
> works like Khaos, with Y pointing down
> ([Godot docs](https://docs.godotengine.org/en/stable/tutorials/math/vector_math.html)).

## Points and vectors

A `Point` is a place: where the player is, where the mouse clicked. A `Vector2` is a movement: how far and in which
direction. Keeping them apart makes the code say what it means.

| You write             | You get   | Use it to                                     |
| :-------------------- | :-------- | :-------------------------------------------- |
| `point - otherPoint`  | `Vector2` | Find the way from one place to another        |
| `point + vector`      | `Point`   | Move a place                                  |
| `point - vector`      | `Point`   | Move a place backward                         |
| `vector * 2f`         | `Vector2` | Make a movement longer or shorter             |

Adding two points does not compile, on purpose: adding two places means nothing.

```csharp
Point player = new(100f, 50f);
Point enemy = new(130f, 10f);

Vector2 toEnemy = enemy - player;                // (30, -40): right and up
float distance = Point.Distance(player, enemy);  // 50

Vector2 velocity = new(120f, -60f);              // pixels per second
Point later = player + (velocity * 0.5f);        // where the player is half a second later
```

To check whether something is in range, compare `Point.DistanceSquared` with the radius times itself. It gives the
same answer as `Distance` without a square root, which matters when hundreds of enemies check every frame.

`Vector2` also has what a 2D game needs on top of .NET:

| Member                              | What it does                                                                |
| :---------------------------------- | :-------------------------------------------------------------------------- |
| `Up`, `Down`, `Left`, `Right`       | The four directions on the screen                                           |
| `NormalizeOrZero()`                 | A length of one, or zero when nothing is pressed (see [NaN](#nan))          |
| `ClampLength(max)`                  | Caps a speed and keeps its direction                                        |
| `MoveTowards(target, maxDistance)`  | Steps toward a target without going past it                                |
| `Rotate(angle)`, `Direction`        | Turns a vector, or gives the angle it points to                             |

When you need the raw numbers, `(Vector2)point` and `(Point)vector` convert one into the other.

## Sizes and rectangles

A `Size` is a width and a height. A `Rect` is a position and a size: `X` and `Y` are its top-left corner, `Right` is
`X + Width` and `Bottom` is `Y + Height`.

```csharp
Rect paddle = new(300f, 560f, 80f, 16f);

bool hit = paddle.Contains(ball);        // edges count as inside
bool touching = paddle.Intersects(wall); // sharing an edge counts as touching
```

A width or a height of zero is fine: the rectangle is a line or a point, and it still counts in a collision. Only a
negative or invalid size makes `IsEmpty` true.

## Whole numbers: the I suffix

`PointI`, `SizeI` and `RectI` hold whole numbers. Use them for what is counted rather than measured: pixels, tiles of a
grid, the size of a window or a texture. The `I` stands for integer.

A whole-number value turns into its floating-point twin by itself, because nothing is lost:

```csharp
SizeI window = new(1280, 720);
Size area = window;   // (1280, 720)
```

The other way loses the decimals, so you choose how:

| Method       | What it does                                          | Use it to                              |
| :----------- | :---------------------------------------------------- | :------------------------------------- |
| `Round()`    | Nearest whole number, halves away from zero (2.5 → 3) | Snap a sprite to the pixel grid        |
| `Floor()`    | Down, even below zero (-0.3 → -1)                     | Find the tile under a position         |
| `Ceiling()`  | Up                                                    | Size a texture that must fit some text |
| `Truncate()` | Toward zero (-0.3 → 0)                                | Rarely the right choice in a game      |

The difference shows left of the origin. With tiles of 32 pixels, a player at x = -10 stands on tile -1:

```csharp
Point player = new(-10f, 70f);

PointI tile = new Point(player.X / 32f, player.Y / 32f).Floor();     // (-1, 2)
PointI wrong = new Point(player.X / 32f, player.Y / 32f).Truncate(); // (0, 2), the tile to the right
```

A `RectI` counts cells, so its right and bottom edges are outside. A 10 × 10 rectangle at the origin covers the cells
0 to 9, and two rectangles that only share an edge do not overlap. That is where `Rect` and `RectI` differ:

```csharp
new Rect(0f, 0f, 10f, 10f).Contains(new Point(10f, 0f));  // true, on the edge
new RectI(0, 0, 10, 10).Contains(new PointI(10, 0));      // false, the first column outside
```

To visit every tile of an area, loop over `Cells`. It goes row by row from the top-left tile:

```csharp
foreach (PointI cell in room.Cells)
    DrawFloor(cell);
```

`Area` on `SizeI` and `RectI` is a `long`, because a big world multiplied out does not fit in an `int`.

## Angles

An `Angle` holds a rotation without asking you to remember the unit. Create it from degrees or radians, and read it
back either way:

```csharp
Angle quarter = Angle.FromDegrees(90f);

float degrees = quarter.Degrees;   // 90
float radians = quarter.Radians;   // 1.5707964
```

A positive angle turns clockwise on the screen. `Normalize()` brings an angle back between 0° and 360°, and
`NormalizeSigned()` between -180° and 180°, which tells you the shortest way to turn:

```csharp
Angle facing = Angle.FromDegrees(350f);
Angle wanted = Angle.FromDegrees(10f);

Angle turn = (wanted - facing).NormalizeSigned();   // 20°: turn clockwise, not 340° the long way
```

To aim at something, take the direction of the vector toward it: `(target - turret).Direction`. To go the other way,
`Vector2.FromAngle(angle, speed)` gives the movement that points in that direction.

## Comparing values

`==` and `Equals` compare exactly. That is what you want for whole numbers. With decimals, two values computed
differently are often a tiny bit apart: adding 0.1 ten times gives 1.0000001, not 1. Compare those with
`IsApproximately` and a tolerance:

```csharp
if (ship.IsApproximately(dock, 0.5f))   // within half a pixel on each axis
    Dock();
```

> [!TIP]
> Coming from Unity? Its `Vector2 ==` is approximate: two vectors are equal when they are less than 0.00001 apart
> ([Unity docs](https://docs.unity.com/en-us/engine/current/script-reference/unityengine/vector2/op-equality)). In
> Khaos, `==` is exact, as in Godot
> ([Godot docs](https://docs.godotengine.org/en/stable/classes/class_vector2.html#class-vector2-operator-eq-vector2)).
> Where your Unity code relied on `==`, use `IsApproximately`.

### Angles and full turns

An angle compares its radians, so 360° is not equal to 0°, even if both point the same way. `IsApproximately` on an
angle wraps around the circle: 359.9° and 0.1° are only 0.2° apart, and 720° points the same way as 0°.

```csharp
Angle full = Angle.FromDegrees(360f);

bool same = full == Angle.Zero;                                          // false
bool sameWay = full.IsApproximately(Angle.Zero, Angle.FromDegrees(1f));  // true
```

### NaN

A calculation that has no answer, such as 0 divided by 0, gives `NaN`, "not a number". Normalizing a zero vector does
it too, which is why `NormalizeOrZero` exists. A `NaN` spreads to everything it touches, so a character with a `NaN`
position simply disappears.

In plain C#, `float.NaN == float.NaN` is `false`. In Khaos, two values with `NaN` at the same place are equal:

```csharp
Point lost = new(float.NaN, 0f);

bool plain = float.NaN == float.NaN;            // false
bool khaos = lost == new Point(float.NaN, 0f);  // true
```

That is the rule of `float.Equals` in .NET, and it lets a `HashSet` or a `Dictionary` find a value again. To find a
`NaN`, test it directly with `float.IsNaN(point.X)`.

A value can also end up as -0, for example -1 × 0. It is equal to 0 everywhere in Khaos, with the same hash code.

## Keep each frame light

`ContainsAll`, `ContainsAny` and `TryEnclose` on `Rect` and `RectI` accept an array, a `List<T>` or any sequence.
An array or a `List<T>` is read directly, without any copy. A LINQ query, such as
`enemies.Select(enemy => enemy.Position)`, creates new objects each time it runs. In code that runs every frame, keep
the positions in an array or a `List<T>` and pass that.
