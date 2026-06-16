# Command Tags

Tag commands and enable or disable them in bulk at runtime.

```csharp
[TerminalCommand("noclip")]
[TerminalTag("debug")]
void NoClip() { ... }
```

```csharp
// Disable all "debug" commands (e.g. in a release build):
terminal.Registry.DisableByTag("debug");

// Re-enable them:
terminal.Registry.EnableByTag("debug");
```

---

## Use cases

- Strip cheat commands from release builds without removing the code
- Toggle feature-flag commands during QA
- Separate editor-only commands from runtime ones

A disabled command is hidden from `help` and won't fire if typed, it's as if it was never registered.
