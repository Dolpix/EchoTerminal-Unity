using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EchoTerminal.Scripts.TerminalCore.Attributes;
using EchoTerminal.Scripts.TerminalCore.Command;
using EchoTerminal.Scripts.TerminalCore.Structs;
using EchoTerminal.Scripts.TerminalCore.Token;

namespace EchoTerminal.Scripts.TerminalCore.Hints
{
public static class CommandHintBuilder
{
	public static List<List<CommandHintSegment>> Build(string input, CommandParseResult result, Tokenizer tokenizer)
	{
		if (result.Entries == null || result.Entries.Count == 0)
		{
			return null;
		}

		List<CommandEntry> candidates = ResolveCandidates(result);
		if (candidates.Count == 0)
		{
			return null;
		}

		int activeSegmentIndex = ResolveActiveSegmentIndex(input, result);

		var rows = new List<List<CommandHintSegment>>();
		foreach (CommandEntry entry in candidates)
		{
			rows.Add(BuildRow(result.CommandToken.Raw, entry, activeSegmentIndex, tokenizer));
		}

		return rows;
	}

	private static List<CommandHintSegment> BuildRow(
		string commandName,
		CommandEntry entry,
		int activeSegmentIndex,
		Tokenizer tokenizer)
	{
		var segments = new List<CommandHintSegment>();
		var segmentIndex = 0;

		segments.Add(new(commandName, segmentIndex++ == activeSegmentIndex));

		if (entry.HasTarget)
		{
			segments.Add(new("@target", segmentIndex++ == activeSegmentIndex));
		}

		foreach (ParameterInfo param in entry.Method.GetParameters())
		{
			if (param.GetCustomAttribute<InjectAttribute>() != null)
			{
				continue;
			}

			string label = ResolveLabel(param, tokenizer);
			segments.Add(new(label, segmentIndex++ == activeSegmentIndex));
		}

		return segments;
	}

	private static int CountUserParams(CommandEntry entry)
	{
		int injected = entry.Method.GetParameters().Count(p => p.GetCustomAttribute<InjectAttribute>() != null);
		int total = entry.Method.GetParameters().Length - injected;
		return entry.HasTarget ? total + 1 : total;
	}

	private static int ResolveActiveSegmentIndex(string input, CommandParseResult result)
	{
		if (result.CommandToken.State == TokenState.Partial || !input.Contains(' '))
		{
			return 0;
		}

		var completedArgs = 0;
		if (result.ArgTokens == null)
		{
			return 1;
		}

		foreach (Token.Token token in result.ArgTokens)
		{
			if (token.State == TokenState.Completed)
			{
				completedArgs++;
			}
			else
			{
				break;
			}
		}

		return 1 + completedArgs;
	}

	private static List<CommandEntry> ResolveCandidates(CommandParseResult result)
	{
		if (result.Entry.HasValue)
		{
			return new() { result.Entry.Value };
		}

		if (result.ArgTokens == null || result.ArgTokens.Count == 0)
		{
			return result.Entries;
		}

		int typedArgCount = result.ArgTokens.Count(t => t.State == TokenState.Completed);
		bool hasPartial =
			result.ArgTokens.Any(t => t.State != TokenState.Completed && t.State != TokenState.Failed);

		return result.Entries.Where(e =>
					 {
						 int paramCount = CountUserParams(e);
						 return hasPartial ? paramCount >= typedArgCount + 1 : paramCount >= typedArgCount;
					 })
					 .ToList();
	}

	private static string ResolveLabel(ParameterInfo param, Tokenizer tokenizer)
	{
		if (tokenizer != null &&
			tokenizer.TryGetParser(param.ParameterType, out ITokenParser parser) &&
			parser is IHintLabeler labeler)
		{
			return $"{labeler.HintLabel}: {param.Name}";
		}

		return param.ParameterType == typeof(string) ? $"\"{param.Name}\"" : param.Name;
	}
}
}