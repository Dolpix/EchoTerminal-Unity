using System;
using System.Collections.Generic;
using EchoTerminal.Scripts.Commands.Bind;
using EchoTerminal.Scripts.ScriptableObjects.Highlighting;
using EchoTerminal.Scripts.TerminalCore.Command;
using EchoTerminal.Scripts.TerminalCore.Enum;
using EchoTerminal.Scripts.TerminalCore.Structs;
using EchoTerminal.Scripts.TerminalCore.Suggestions;
using EchoTerminal.Scripts.TerminalCore.Targets;
using EchoTerminal.Scripts.TerminalCore.Token;
using EchoTerminal.Scripts.TerminalCore.Token.TokenParser;
using EchoTerminal.Scripts.UI.EchoComponents;
using UnityEngine;

namespace EchoTerminal.Scripts.TerminalCore
{
public class Terminal
{
	public CommandParser CommandParser { get; }

	public IReadOnlyList<TerminalEntry> Entries => _entries;
	public CommandRegistry Registry { get; }
	public InjectionContainer Injector { get; }
	public SuggestorRegistry Suggestors { get; }
	public Tokenizer Tokenizer { get; }
	public event Action OnCleared;
	public event Action<TerminalEntry> OnEntryAdded;
	public event Action OnSubmitted;
	public event Action<string> OnCommandSubmitted;
	public TerminalHighlighterCore HighlighterCore { get; }
    public HashSet<string> SilencedInputs { get; } = new(StringComparer.OrdinalIgnoreCase);
	private readonly List<TerminalEntry> _entries = new();
	private readonly CommandExecutor _executor;
	private readonly int _maxEntries;

	public Terminal(HighlighterSet highlighterSet = null, int maxEntries = 1000)
	{
		_maxEntries = maxEntries;
		Injector = new();
		Injector.Register(() => this);
		Registry = new();
		Registry.Scan();
		var targetProvider = new SceneTargetProvider(Registry);
		Suggestors = new();
		Suggestors.Scan(Registry, targetProvider);
		ParserRegistry.Register<CommandNameParser>(() => new CommandNameParser(() => Registry.GetCommandNames()));
		ParserRegistry.Register<TargetParser>(() => new TargetParser(targetProvider));
		Tokenizer = new(ParserRegistry.CreateAllParsers());
		CommandParser = new(Registry, Tokenizer);
		Suggestors.InitComplexSuggesters(Registry, CommandParser);
		_executor = new(CommandParser, Registry, Tokenizer, Injector);
        _executor.SetSilencedInputs(SilencedInputs);
		HighlighterCore = new(CommandParser, Tokenizer, highlighterSet);
		BindCommand.Terminal = this;
	}

	public void Clear()
	{
		_entries.Clear();
		OnCleared?.Invoke();
	}

	public void Log(string text, Color? color = null, LogKind kind = LogKind.Log)
	{
		var entry = new TerminalEntry(text, color ?? Color.white, kind);
		if (_entries.Count >= _maxEntries)
		{
			_entries.RemoveAt(0);
		}

		_entries.Add(entry);
		OnEntryAdded?.Invoke(entry);
	}

	public void Submit(string input)
	{
		OnCommandSubmitted?.Invoke(input);
		OnSubmitted?.Invoke();
        bool silenced = SilencedInputs.Contains(input.Trim());
        string logText = silenced ? input : HighlighterCore.BuildHighlightedText(input);
		Log(logText, kind: LogKind.Command);
		_executor.Execute(input);
	}
}
}