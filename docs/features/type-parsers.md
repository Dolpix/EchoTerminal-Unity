# Type Parsers

Add support for a custom argument type by implementing `ITokenParser` in the `EchoTerminal.TerminalCore` namespace. `ParserRegistry` discovers it at startup, no registration needed.
Recommend looking at other parsers to get a good idea on how to write one of these. Its also important not to have simmilar rules to other types or the parser can get confused on what type you are trying to parse.

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

---

## TokenState values

| Value | Meaning |
|-------|---------|
| `Completed` | Input is valid and fully parsed |
| `Partial` | Input looks like this type but is incomplete |
| `Failed` | Input does not match this type |

`Partial` drives the inline hint color in the terminal, use it when the user is mid-way through a valid value (e.g. typing an item ID that exists in the database prefix).
