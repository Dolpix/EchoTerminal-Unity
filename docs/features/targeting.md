# GameObject Targeting

Any command parameter of type `Target` gets `@name` autocomplete populated from the active scene. `@all` matches every registered instance.

```csharp
[TerminalCommand("freeze")]
[TerminalTarget]
void Freeze(Target target)
{
    // target.Value holds "@player", "@enemy_03", "@all", etc.
}
```

---

## How the target list is built

`SceneTargetProvider` collects all active MonoBehaviours registered in `CommandRegistry` and exposes their GameObject names as `@name` suggestions. The list is cached per-frame and only rebuilt when the frame counter changes.

---

## @all

`@all` is a reserved keyword. When passed, the command fires on every registered instance rather than a specific one. No extra handling needed in your command — the dispatch layer handles it.

---

Next: [UI Components →](ui-components.md)
