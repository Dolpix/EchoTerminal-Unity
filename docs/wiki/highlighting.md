# Highlighting Themes

Create a `HighlighterSet` asset via `Create > Echo Terminal > Highlighter Set`. Each entry maps a C# type to a `TokenHighlighter`. Built-in options: flat color, bool-specific coloring, and rainbow.

Assign the set to your `TerminalConfig` to apply it.

---

## Custom highlighter

```csharp
[CreateAssetMenu(menuName = "Echo Terminal/Custom Highlighter")]
public class WarnHighlighter : TokenHighlighter
{
    public override string Apply(string text) => $"<color=#FF4400>{text}</color>";
}
```

`Apply` receives the raw token string and returns a TMP rich-text wrapped version. Keep it to a single `<color>` or `<b>` wrap — nesting multiple tags can break TMP layout.

---

Next: [Command Tags →](command-tags.md)
