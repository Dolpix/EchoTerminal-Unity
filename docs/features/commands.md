# Commands

Attach `[TerminalCommand]` to any static or MonoBehaviour method. `Terminal` discovers it automatically at startup, no manual registration needed.

```csharp
[TerminalCommand("setspeed")]
void SetSpeed(float speed) { ... }
```

---

## Supported argument types

These are parsed out of the box:

| Type | Example input |
|------|--------------|
| `int` | `42` |
| `float` | `3.14` |
| `bool` | `true` / `false` |
| `string` | `hello` |
| `Vector2` | `1 2` |
| `Vector3` | `1 2 3` |
| `Color` | `#FF0000` or `red` |
| `Quaternion` | `0 0 0 1` |
| `Rect` | `0 0 100 100` |
| `List<T>` | space-separated values |

For types not in this list, see [Type Parsers](type-parsers.md).

---

## Multiple instances

If multiple active MonoBehaviours in the scene have the same command registered, the terminal fires on all of them. This is intentional, useful for broadcasting to every enemy, every spawner, etc.
