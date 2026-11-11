// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Events;

internal sealed class CustomEventSlots<T>(SDL_EventType type, int capacity) : CustomEventSlots(type) where T : struct
{
    private readonly ushort[] _generations = new ushort[capacity];
    private readonly Lock _lock = new();
    private readonly T[] _payloads = new T[capacity];
    private readonly bool[] _used = new bool[capacity];

    private int _next;

    internal int Capacity => _payloads.Length;

    internal override void Release(int code)
    {
        lock (_lock)
        {
            if (!IsCurrent(code, out int slot))
                return;

            Free(slot);
        }
    }

    internal override void ReleaseAll()
    {
        lock (_lock)
        {
            for (int slot = 0; slot < _used.Length; slot++)
            {
                if (_used[slot])
                    Free(slot);
            }

            _next = 0;
        }
    }

    internal bool TryAdd(in T payload, out int code)
    {
        lock (_lock)
        {
            for (int i = 0; i < _used.Length; i++)
            {
                int slot = (_next + i) % _used.Length;

                if (_used[slot])
                    continue;

                _used[slot] = true;
                _payloads[slot] = payload;
                _next = slot + 1;

                code = slot | (_generations[slot] << GenerationShift);
                return true;
            }
        }

        code = 0;
        return false;
    }

    internal bool TryRead(int code, out T payload)
    {
        lock (_lock)
        {
            if (IsCurrent(code, out int slot))
            {
                payload = _payloads[slot];
                return true;
            }
        }

        payload = default;
        return false;
    }

    private void Free(int slot)
    {
        _payloads[slot] = default;
        _used[slot] = false;
        _generations[slot]++;
    }

    private bool IsCurrent(int code, out int slot)
    {
        slot = code & SlotMask;
        int generation = (int)((uint)code >> GenerationShift);

        return slot < _used.Length && _used[slot] && _generations[slot] == generation;
    }
}

internal abstract class CustomEventSlots(SDL_EventType type)
{
    internal const int MaxCapacity = 1 << GenerationShift;

    private protected const int GenerationShift = 16;
    private protected const int SlotMask = MaxCapacity - 1;

    internal SDL_EventType Type { get; } = type;

    internal abstract void Release(int code);

    internal abstract void ReleaseAll();
}
