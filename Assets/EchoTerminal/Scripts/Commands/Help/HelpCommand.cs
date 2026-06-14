using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EchoTerminal.Scripts.ScriptableObjects.Highlighting;
using EchoTerminal.Scripts.TerminalCore;
using EchoTerminal.Scripts.TerminalCore.Attributes;
using EchoTerminal.Scripts.TerminalCore.Structs;
using EchoTerminal.Scripts.TerminalCore.Token;
using EchoTerminal.Scripts.TerminalCore.Token.TokenParser;

namespace EchoTerminal.Scripts.Commands
{
public static class HelpCommand
{
	private const string _colClassName = "#C9D1D9";
	private const string _colDescription = "#6E8FA8";
	private const string _colSeparator = "#4A6274";

	private const string _indent1 = "    ";
	private const string _separator = "  →  ";

	[TerminalCommand]
	[TerminalDescription("Show all registered commands")]
	private static void Help([Inject] Terminal terminal)
	{
		Tokenizer tokenizer = terminal.Tokenizer;
		IReadOnlyCollection<string> names = terminal.Registry.GetCommandNames();

		if (names.Count == 0)
		{
			terminal.Log("No commands registered.");
			return;
		}

		HighlighterSet hs = terminal.HighlighterCore.HighlighterSet;

		var byClass = new SortedDictionary<string, List<(string name, CommandEntry entry)>>(StringComparer.OrdinalIgnoreCase);

		foreach (string name in names)
		{
			if (!terminal.Registry.TryGet(name, out List<CommandEntry> entries))
			{
				continue;
			}

			foreach (CommandEntry entry in entries)
			{
				string className = entry.Method.DeclaringType?.Name ?? "Unknown";
				if (!byClass.TryGetValue(className, out List<(string name, CommandEntry entry)> list))
				{
					list = new();
					byClass[className] = list;
				}

				list.Add((name, entry));
			}
		}

		var output = new StringBuilder();
		var firstGroup = true;

		foreach ((string className, List<(string name, CommandEntry entry)> entries) in byClass)
		{
			if (!firstGroup)
			{
				output.AppendLine(" ");
			}

			firstGroup = false;

			output.AppendLine($"<color={_colClassName}>{className}</color>");

			List<(string name, CommandEntry entry)> sorted = entries.OrderBy(e => e.entry.Method.MetadataToken).ToList();

			int maxLen = sorted.Max(e => VisibleLength(e.name, e.entry, tokenizer));

			foreach ((string name, CommandEntry entry) in sorted)
			{
				output.AppendLine(BuildLine(name, entry, hs, maxLen, tokenizer));
			}
		}

		terminal.Log(output.ToString().TrimEnd());
	}

	private static string BuildLine(string name, CommandEntry entry, HighlighterSet hs, int padToLength, Tokenizer tokenizer)
	{
		var sb = new StringBuilder();
		sb.Append(_indent1);
		sb.Append('-');
		sb.Append(Colorize(name, typeof(CommandName), hs));

		if (entry.HasTarget)
		{
			sb.Append(' ');
			sb.Append(Colorize("<@target>", typeof(Target), hs));
		}

		foreach (ParameterInfo param in entry.Method.GetParameters())
		{
			if (param.GetCustomAttribute<InjectAttribute>() != null)
			{
				continue;
			}

			sb.Append(' ');
			sb.Append(Colorize($"<{ResolveHintLabel(param, tokenizer)} {param.Name}>", param.ParameterType, hs));
		}

		string desc = entry.Method.GetCustomAttribute<TerminalDescriptionAttribute>()?.Description;
		if (string.IsNullOrEmpty(desc))
		{
			return sb.ToString();
		}

		int visLen = VisibleLength(name, entry, tokenizer);
		int padding = padToLength - visLen;
		sb.Append(new string(' ', padding));
		sb.Append($"<color={_colSeparator}>{_separator}</color>");
		sb.Append($"<color={_colDescription}>{desc}</color>");

		return sb.ToString();
	}

	private static int VisibleLength(string name, CommandEntry entry, Tokenizer tokenizer)
	{
		int len = _indent1.Length + 1 + name.Length;

		if (entry.HasTarget)
		{
			len += 1 + "<@target>".Length;
		}

		foreach (ParameterInfo param in entry.Method.GetParameters())
		{
			if (param.GetCustomAttribute<InjectAttribute>() != null)
			{
				continue;
			}

			len += 1 + $"<{ResolveHintLabel(param, tokenizer)} {param.Name}>".Length;
		}

		return len;
	}

	private static string ResolveHintLabel(ParameterInfo param, Tokenizer tokenizer)
	{
		if (tokenizer != null &&
			tokenizer.TryGetParser(param.ParameterType, out ITokenParser parser) &&
			parser is IHintLabeler labeler)
		{
			return labeler.HintLabel;
		}

		return param.ParameterType == typeof(string) ? "string" : param.ParameterType.Name;
	}

	private static string Colorize(string text, Type tokenType, HighlighterSet hs)
	{
		if (hs == null)
		{
			return text;
		}

		TokenHighlighter highlighter;
		if (tokenType != null)
		{
			hs.TryGet(tokenType, out highlighter);
		}
		else
		{
			highlighter = hs.DefaultHighlighter;
		}

		if (highlighter == null)
		{
			return text;
		}

		var token = new Token { Raw = text, State = TokenState.Completed, ExpectedType = tokenType };
		return highlighter.Highlight(text, token);
	}
}
}