# Autocomplete & Suggestions

## Type-based suggester

Implement `ISuggester` and mark it `[SuggestorFor(typeof(YourType))]`. The registry finds it automatically — no registration needed.

```csharp
[SuggestorFor(typeof(ItemId))]
public class ItemSuggester : ISuggester
{
    public IReadOnlyList<string> GetSuggestions(string partial, Type expectedType = null)
        => ItemDatabase.AllIds().Where(id => id.StartsWith(partial)).ToList();
}
```

Suggestions appear as tab-completable inline hints while the user types.

---

## Inline fixed options

For a fixed set of options on a single parameter, use `[Suggest]` directly on the attribute — no separate class needed:

```csharp
[TerminalCommand("setdifficulty")]
void SetDifficulty([Suggest("easy", "normal", "hard")] string level) { }
```

---

Next: [GameObject Targeting →](targeting.md)
