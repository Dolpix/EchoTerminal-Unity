using System.Collections.Generic;
using EchoTerminal.Scripts.ScriptableObjects;
using EchoTerminal.Scripts.TerminalCore;
using EchoTerminal.Scripts.UI.EchoComponents;
using UnityEngine.UIElements;

namespace EchoTerminal.Scripts.UI
{
public class TerminalView
{
	public Terminal Terminal { get; }

	private readonly List<IEchoComponent> _components = new();

	public TerminalView(VisualElement root, TerminalConfig config)
	{
		Terminal = new(config?.HighlighterSet);

		var logDisplay = new TerminalLogDisplay(Terminal, root, config);
		_components.Add(logDisplay);
		_components.Add(new TerminalAutoScroll(Terminal, root));
		_components.Add(new TerminalSuggestions(Terminal, root, config));
		_components.Add(new TerminalInput(Terminal, root));
		_components.Add(new TerminalInputHistory(Terminal, root));
		_components.Add(new TerminalHighlighter(Terminal, root));
		_components.Add(new TerminalHint(Terminal, root));
		_components.Add(new TerminalToolbar(Terminal, root));
		_components.Add(new TerminalCopyButton(Terminal, root));
		_components.Add(new TerminalLogFilter(Terminal, root));
		_components.Add(new TerminalSearch(Terminal, root));
		_components.Add(new TerminalCollapseButton(logDisplay, root));
		_components.Add(new TerminalUnityLog(Terminal));

		if (root.Q<VisualElement>("game-window") == null)
		{
			return;
		}

		if (config != null)
		{
			_components.Add(new TerminalGameWindow(root, config.CursorSet, config.DragConstraints));
		}
	}

	~TerminalView()
	{
		_components.Clear();
	}

	public void AddComponent(IEchoComponent component)
	{
		_components.Add(component);
	}
}
}