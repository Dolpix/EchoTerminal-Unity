# Echo Terminal

![Echo Terminal](docs/echo_main_render.png)

[![Unity](https://img.shields.io/badge/Unity-6000.x-black?logo=unity&logoColor=white)](https://unity.com)
[![Language](https://img.shields.io/badge/language-C%23-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Open Source](https://img.shields.io/badge/open%20source-Apache%202.0-brightgreen)](https://opensource.org/licenses/Apache-2.0)

### Developer message

I've seen a lot of paid dev consoles and it got me thinking - why isn't there a free one that's actually well built? So I took some time and made this. No price tag, no bloat, just a clean lib you can read, own, and modify.

Every layer is yours. Keep the default UI, swap out just the parts you don't like, or gut the whole thing and wire the backend into something completely your own. The tokenizer, the parser, the suggesters, the highlighting - all of it is open. That's what open source is supposed to mean. Your tools should belong to you, not a license agreement.

Grab it, break it, make something awesome with it, and share it with your dev friends. Open to any questions, issues, or ideas you've got!

If it saves you some time and you can spare it, [paying me](https://github.com/sponsors/Dolpix) would be sick :)

Make games and become great!

Signed Dolpix



## Getting Started

### Requirements

- Unity 6 (6000.x)
- UI Toolkit (included in Unity 6)
- TextMeshPro

### Installation

Download the unity package and install it, OR copy the `Assets/EchoTerminal` folder from this repo into your project.

> You don't need the `Assets/EchoTerminal/Demo` or `Assets/EchoTerminal/Editor/Tests` folders.

### Add the terminal to a scene

1. Add the `EchoTerminal` prefab into your scene.
2. Press Play.
3. Press `~` to open the terminal.
4. Type `help` to see registered commands.

### Your first command

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

`Terminal` scans all assemblies at startup. Type `heal 50` in the console and it fires on every active instance in the scene. No registration step needed.



## Examples

<!-- Replace these with actual GIFs once recorded -->
![Registering and running a command](docs/gif_basic_command.gif)
![Tab complete and syntax highlighting](docs/gif_autocomplete.gif)



## Wiki

Full documentation — commands, type parsers, autocomplete, targeting, UI components, highlighting themes, tags, and inputs:

**[dolpix.github.io/EchoTerminal](https://dolpix.github.io/EchoTerminal)**



## Contributing

Bug reports and feature requests go in [Issues](https://github.com/Dolpix/EchoTerminal/issues).

Pull requests are welcome. One problem per PR. If something in the code or the docs is confusing, open an issue.



## Support

This project is free and open source. If it saved you time or helped you ship, consider supporting it:

- [GitHub Sponsors](https://github.com/sponsors/Dolpix)

Engineers and artists should own their tools. Take this one.

Apache 2.0 — use it, fork it, ship it.
