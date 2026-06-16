# GameObject Targeting

Implementing both the `TerminalCommand` and `TerminalTarget` attributes will allow for you to have a specific MonoBehavior you can target. `@all` matches every registered instance.
It is important to note that gameobjects should try to avoid spaces in name as the current parser pattern doesnt recognise them.

```csharp
[TerminalCommand("freeze")]
[TerminalTarget]
void Freeze()
{
    // target.Value holds "@player", "@enemy_03", "@all", etc.
}
```

---

## How the target list is built

`SceneTargetProvider` collects all active MonoBehaviours registered in `CommandRegistry` and exposes their GameObject names as `@name` suggestions. The list is cached per-frame and only rebuilt when the frame counter changes.

---

## @all

`@all` is a reserved keyword. When passed, the command fires on every registered instance rather than a specific one. No extra handling needed in your command, the dispatch layer handles it.
