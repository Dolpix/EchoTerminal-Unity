# Common Tasks

Quick recipes for the most common patterns. Each links to the full feature page for details.

---

## Register a command

Attach `[TerminalCommand]` to any static or instance method on a MonoBehaviour. No registration step needed - `Terminal` discovers it at startup.

```csharp
[TerminalCommand("setspeed")]
void SetSpeed(float speed)
{
    _rb.velocity = Vector3.forward * speed;
}
```

Full reference: [Commands ](features/commands.md)

---

## Add a description to a command

Add `[TerminalDescription]` alongside `[TerminalCommand]`. The description shows up in the `help` output so users know what the command does.

```csharp
[TerminalCommand("setspeed")]
[TerminalDescription("Set the player's movement speed.")]
void SetSpeed(float speed)
{
    _rb.velocity = Vector3.forward * speed;
}
```

Full reference: [Commands ](features/commands.md)

---

## Add a custom argument type

If you need a custom type to be recognised by the system. Implement `ITokenParser` in the `EchoTerminal.TerminalCore` namespace. The parser registry discovers it automatically.

```csharp
namespace EchoTerminal.TerminalCore
{
    public class ItemIdParser : ITokenParser
    {
        public Type Type => typeof(ItemId);

        public TokenState ParseTokenState(string raw, Type expectedType = null)
        {
            return ItemDatabase.Contains(raw) ? TokenState.Completed : TokenState.Failed;
        }

        public object ParseValue(string raw, Type expectedType = null)
        {
            return new ItemId(raw);
        }
    }
}
```

Full reference: [Type Parsers](features/type-parsers.md)

---

## Add tab complete for a custom type

Implement `ISuggester` and mark it with `[SuggestorFor(typeof(YourType))]`.

```csharp
[SuggestorFor(typeof(ItemId))]
public class ItemSuggester : ISuggester
{
    public IReadOnlyList<string> GetSuggestions(string partial, Type expectedType = null)
    {
        return ItemDatabase.AllIds().Where(id => id.StartsWith(partial)).ToList();
    }
}
```

Full reference: [Autocomplete & Suggestions](features/autocomplete.md)

---

## Target a specific GameObject

Use the `TerminalTarget` attribute to target specific MonoBehaviors. Commands that include it get `@name` autocomplete from the active scene. Try not to have spaces in your gameobject names as this messes with the syntax

```csharp
[TerminalCommand("kill")]
[TerminalTarget]
void Kill()
{
    // target.Value is "@player", "@enemy_03", "@all", etc.
}
```

Full reference: [GameObject Targeting](features/targeting.md)

---

## Tag commands

`[TerminalTag]` marks a command with a label. Tags are just strings, group commands however makes sense for your project. A command can have multiple tags.

```csharp
[TerminalCommand("noclip")]
[TerminalTag("debug")]
void NoClip()
{
    _controller.noClip = !_controller.noClip;
}
```

Tags don't do anything on their own, they're a way to organize commands so you can act on groups of them at runtime.

Full reference: [Command Tags](features/command-tags.md)

---

## Enable or disable commands by tag

Use `DisableByTag` and `EnableByTag` on the registry to toggle entire groups at once. A disabled command is hidden from `help` and won't fire if typed.

```csharp
// Disable all commands tagged "debug":
terminal.Registry.DisableByTag("debug");

// Re-enable them:
terminal.Registry.EnableByTag("debug");
```

Common uses: strip cheat commands from release builds, toggle feature-flag commands during QA, hide editor-only commands at runtime. The tag system is open-ended - use it for whatever grouping makes sense in your project.

Full reference: [Command Tags](features/command-tags.md)

---

## Remap the open key

Open the input asset under the `Inputs` folder. It uses Unity's Input System, any supported binding works.

Default is `~` (Backquote). Full reference: [Inputs ](features/inputs.md)

---

## Customize syntax highlight colors

Create a `HighlighterSet` asset via `Create > Echo Terminal > Highlighter Set`, assign colors per type, then set it on your `TerminalConfig`.

Full reference: [Highlighting Themes](features/highlighting.md)
