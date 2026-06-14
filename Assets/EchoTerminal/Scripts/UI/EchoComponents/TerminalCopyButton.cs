using System.Text;
using System.Text.RegularExpressions;
using EchoTerminal.Scripts.TerminalCore;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoTerminal.Scripts.UI.EchoComponents
{
public class TerminalCopyButton : IEchoComponent
{
	private static readonly Regex RichTextTags = new("<[^>]+>", RegexOptions.Compiled);

	private readonly Button _copyButton;
	private readonly Label _copyToast;
	private readonly VisualElement _logContainer;

	public TerminalCopyButton(Terminal terminal, VisualElement root)
	{
		_logContainer = root?.Q<VisualElement>("log-container");
		_copyButton = root?.Q<Button>("copy-button");

		_copyToast = new("Copied to clipboard");
		_copyToast.AddToClassList("terminal-copy-toast");
		_copyToast.pickingMode = PickingMode.Ignore;
		VisualElement toastParent = root?.Q<VisualElement>("terminal-content") ?? root;
		toastParent?.Add(_copyToast);

		_copyButton?.RegisterCallback<ClickEvent>(OnCopyClicked);
	}

	~TerminalCopyButton()
	{
		_copyButton?.UnregisterCallback<ClickEvent>(OnCopyClicked);
	}

	private void OnCopyClicked(ClickEvent evt)
	{
		if (_logContainer == null)
		{
			return;
		}

		bool showTimestamp = _logContainer.ClassListContains("timestamps-visible");
		var sb = new StringBuilder();

		foreach (VisualElement row in _logContainer.Children())
		{
			if (row.resolvedStyle.display == DisplayStyle.None)
			{
				continue;
			}

			var timestamp = row.Q<Label>("timestamp");
			var message = row.Q<Label>("message");
			var badge = row.Q<Label>("collapse-badge");

			if (message == null)
			{
				continue;
			}

			if (showTimestamp && timestamp != null)
			{
				sb.Append($"[{timestamp.text}] ");
			}

			if (badge != null && badge.resolvedStyle.display != DisplayStyle.None && !string.IsNullOrEmpty(badge.text))
			{
				sb.Append($"[{badge.text}] ");
			}

			sb.Append(RichTextTags.Replace(message.text, ""));
			sb.AppendLine();
		}

		GUIUtility.systemCopyBuffer = sb.ToString().TrimEnd();

		_copyToast.AddToClassList("terminal-copy-toast--visible");
		_copyToast.schedule.Execute(() => _copyToast.RemoveFromClassList("terminal-copy-toast--visible")).StartingIn(1500);
	}
}
}