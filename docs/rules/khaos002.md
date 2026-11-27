# KHAOS002: [CustomEvents] group has no events

| Item     | Value          |
| :------- | :------------- |
| Category | Events         |
| Severity | Error          |
| Where    | Your game code |

## Cause

A struct marked `[CustomEvents]` lists no event.

## Why

The events of a group are the `readonly record struct` types listed in `[CustomEvents]`. A group with none gives
`OnEvent` nothing to match.

## Reported

```csharp
[CustomEvents]
public readonly partial struct ShopEvents;
//                             ~~~~~~~~~~ KHAOS002: 'ShopEvents' is marked [CustomEvents] but lists no
//                                        events; list them as [CustomEvents(typeof(MyEvent))]
```

## Fix

```csharp
public readonly record struct ItemBought(int ItemId, int Price);

[CustomEvents(typeof(ItemBought))]
public readonly partial struct ShopEvents;
```

## Suppressing

Not possible: list an event, or remove `[CustomEvents]`.
