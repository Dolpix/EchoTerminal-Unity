# Common Tasks

Quick recipes for the most common patterns. Each links to the full feature page for details.

---

## Register a command

Attach `[TerminalCommand]` to any static or instance method on a MonoBehaviour. No registration step needed — `Terminal` discovers it at startup.

```csharp
[TerminalCommand("setspeed")]
void SetSpeed(float speed)
{
    _rb.velocity = Vector3.forward * speed;
}
```

Full reference: [Commands →](features/commands.md)

---

## Add a custom argument type

Implement `ITokenParser` in the `EchoTerminal.TerminalCore` namespace. The parser registry discovers it automatically.

```csharp
namespace EchoTerminal.TerminalCore
{
    public class ItemIdParser : ITokenParser
    {
        public Type Type => typeof(ItemId);

        public TokenState ParseTokenState(string raw, Type expectedType = null)
            => ItemDatabase.Contains(raw) ? TokenState.Completed : TokenState.Failed;

        public object ParseValue(string raw, Type expectedType = null)
            => new ItemId(raw);
    }
}
```

Full reference: [Type Parsers →](features/type-parsers.md)

---

## Add tab complete for a custom type

Implement `ISuggester` and mark it with `[SuggestorFor(typeof(YourType))]`.

```csharp
[SuggestorFor(typeof(ItemId))]
public class ItemSuggester : ISuggester
{
    public IReadOnlyList<string> GetSuggestions(string partial, Type expectedType = null)
        => ItemDatabase.AllIds().Where(id => id.StartsWith(partial)).ToList();
}
```

Full reference: [Autocomplete & Suggestions →](features/autocomplete.md)

---

## Target a specific GameObject

Use the `Target` type as a parameter. Commands that include it get `@name` autocomplete from the active scene.

```csharp
[TerminalCommand("kill")]
[TerminalTarget]
void Kill(Target target)
{
    // target.Value is "@player", "@enemy_03", "@all", etc.
}
```

Full reference: [GameObject Targeting →](features/targeting.md)

---

## Disable commands in a release build

Tag commands with `[TerminalTag]` and disable the tag at runtime.

```csharp
[TerminalCommand("noclip")]
[TerminalTag("debug")]
void NoClip() { ... }
```

```csharp
// In your release build setup:
terminal.Registry.DisableByTag("debug");
```

Disabled commands are hidden from `help` and will not fire. Full reference: [Command Tags →](features/command-tags.md)

---

## Remap the open key

Open the input asset under the `Inputs` folder. It uses Unity's Input System — any supported binding works.

Default is `~` (Backquote). Full reference: [Inputs →](features/inputs.md)

---

## Customize syntax highlight colors

Create a `HighlighterSet` asset via `Create > Echo Terminal > Highlighter Set`, assign colors per type, then set it on your `TerminalConfig`.

Full reference: [Highlighting Themes →](features/highlighting.md)
