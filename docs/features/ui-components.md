# UI Components

`TerminalView` holds a list of `IEchoComponent`. Each component subscribes to `Terminal` events independently. Add your own after construction:

```csharp
public class StatusBar : IEchoComponent
{
    public StatusBar(Terminal terminal, VisualElement root)
    {
        var label = root.Q<Label>("status-bar");
        terminal.OnEntryAdded += entry => label.text = entry.Text;
    }
}

// In your setup:
var view = new TerminalView(root, config);
view.AddComponent(new StatusBar(view.Terminal, root));
```

---

## Replacing the layout

Replace the UXML entirely to change the terminal layout. Only include `IEchoComponent` types that match the elements present in your markup, components that query for a missing element will no-op or error depending on implementation.

---

## Built-in components

| Component | What it does |
|-----------|-------------|
| `LogView` | Scrollable output log |
| `InputField` | Text input with hint overlay |
| `SuggestionList` | Tab-complete dropdown |
| `HighlightOverlay` | Live TMP syntax coloring |
