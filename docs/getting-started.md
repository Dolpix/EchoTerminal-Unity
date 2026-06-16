# Getting Started

## Requirements

- Unity 6 (6000.x)
- UI Toolkit (included in Unity 6)
- TextMeshPro

{% hint style="info" %}
Echo Terminal may work on earlier Unity versions but has only been tested on Unity 6. If you hit issues on a specific version, [open a ticket](https://github.com/Dolpix/EchoTerminal/issues).
{% endhint %}

---

## Installation

Download the Unity package and install it, **or** copy the `Assets/EchoTerminal` folder from the repo into your project.

{% hint style="success" %}
You don't need the `Assets/EchoTerminal/Demo` or `Assets/EchoTerminal/Editor/Tests` folders.
{% endhint %}

---

## Add the terminal to a scene

**1. Add the `EchoTerminal` prefab into your scene.**

![Add prefab to scene](assets/getting-started/step-1-add-prefab.gif)

**2. Press Play.**

![Press Play](assets/getting-started/step-2-press-play.gif)

**3. Press `~` to open the terminal.**

![Open the terminal](assets/getting-started/step-3-open-terminal.gif)

**4. Type `help` to see all registered commands.**

![Help command](assets/getting-started/step-4-help-command.gif)

For editor tooling, use `EditorTerminalUI` inside an `EditorWindow` instead.

---

## Your first command

```csharp
public class PlayerCommands : MonoBehaviour
{
    [TerminalCommand("heal")]
    void Heal(float amount)
    {
        GetComponent<Health>().Add(amount);
    }
}
```

`Terminal` scans all assemblies at startup. Type `heal 50` in the console and this method fires on every active instance in the scene. No registration step needed.
