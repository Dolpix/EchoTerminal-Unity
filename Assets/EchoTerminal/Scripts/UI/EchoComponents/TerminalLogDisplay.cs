using EchoTerminal.Scripts.ScriptableObjects;
using EchoTerminal.Scripts.TerminalCore;
using EchoTerminal.Scripts.TerminalCore.Enum;
using EchoTerminal.Scripts.TerminalCore.Structs;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoTerminal.Scripts.UI.EchoComponents
{
public class TerminalLogDisplay : IEchoComponent
{
	private readonly VisualElement _container;
	private readonly VisualTreeAsset _entryTemplate;
	private readonly Terminal _terminal;
	private int _entryCount;
	private bool _collapseEnabled;

	public TerminalLogDisplay(Terminal terminal, VisualElement root, TerminalConfig config)
	{
		_terminal = terminal;
		_container = root?.Q<VisualElement>("log-container");
		_entryTemplate = config.LogEntryTemplate;

		if (_container == null)
		{
			return;
		}

		_terminal.OnEntryAdded += OnEntryAdded;
		_terminal.OnCleared += OnCleared;
		Refresh();
	}

	~TerminalLogDisplay()
	{
		_terminal.OnEntryAdded -= OnEntryAdded;
		_terminal.OnCleared -= OnCleared;
	}

	public void SetCollapse(bool enabled)
	{
		if (_collapseEnabled == enabled)
		{
			return;
		}

		_collapseEnabled = enabled;
		Refresh();
	}

	private void OnEntryAdded(TerminalEntry entry)
	{
		if (_collapseEnabled)
		{
			TryMergeOrAdd(entry);
		}
		else
		{
			AddEntryElement(entry);
		}
	}

	private void OnCleared()
	{
		_container?.Clear();
		_entryCount = 0;
	}

	private void Refresh()
	{
		_container?.Clear();
		_entryCount = 0;

		if (_collapseEnabled)
		{
			foreach (TerminalEntry entry in _terminal.Entries)
			{
				TryMergeOrAdd(entry);
			}
		}
		else
		{
			foreach (TerminalEntry entry in _terminal.Entries)
			{
				AddEntryElement(entry);
			}
		}
	}

	private void TryMergeOrAdd(TerminalEntry entry)
	{
		if (_container == null)
		{
			return;
		}

		foreach (VisualElement child in _container.Children())
		{
			if (child.userData is not CollapseData data || data.Text != entry.Text || data.Kind != entry.Kind)
			{
				continue;
			}

			data.Count++;
			var badge = child.Q<Label>("collapse-badge");
			if (badge != null)
			{
				badge.text = $"x{data.Count}";
				badge.style.display = DisplayStyle.Flex;
			}

			var timestamp = child.Q<Label>("timestamp");
			if (timestamp != null)
			{
				timestamp.text = entry.Timestamp.ToString("HH:mm:ss");
			}

			return;
		}

		AddEntryElement(entry);
	}

	private void AddEntryElement(TerminalEntry entry)
	{
		if (_container == null)
		{
			return;
		}

		TemplateContainer clone = _entryTemplate.CloneTree();
		var row = clone.Q<VisualElement>(className: "terminal-entry");
		row.AddToClassList(_entryCount % 2 == 0 ? "terminal-entry--even" : "terminal-entry--odd");
		row.AddToClassList($"terminal-entry--{entry.Kind.ToString().ToLower()}");

		row.userData = new CollapseData { Text = entry.Text, Kind = entry.Kind, Count = 1 };

		var timestamp = clone.Q<Label>("timestamp");
		timestamp.text = entry.Timestamp.ToString("HH:mm:ss");
		timestamp.selection.isSelectable = true;

		var message = clone.Q<Label>("message");
		message.enableRichText = true;
		message.selection.isSelectable = true;
		string hex = ColorUtility.ToHtmlStringRGB(entry.Color);
		message.text = $"<color=#{hex}>{entry.Text}</color>";

		var badge = new Label { name = "collapse-badge" };
		badge.AddToClassList("terminal-entry-badge");
		badge.style.display = DisplayStyle.None;
		row.Add(badge);

		_container.Add(row);
		_entryCount++;
	}

	private class CollapseData
	{
		public string Text;
		public LogKind Kind;
		public int Count;
	}
}
}