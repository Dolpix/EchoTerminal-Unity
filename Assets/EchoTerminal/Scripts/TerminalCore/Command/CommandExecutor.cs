using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EchoTerminal.Scripts.TerminalCore.Attributes;
using EchoTerminal.Scripts.TerminalCore.Structs;
using EchoTerminal.Scripts.TerminalCore.Token;
using EchoTerminal.Scripts.TerminalCore.Token.TokenParser;
using UnityEngine;

namespace EchoTerminal.Scripts.TerminalCore.Command
{
public class CommandExecutor
{
	private readonly CommandParser _commandParser;
	private readonly CommandRegistry _registry;
	private readonly Tokenizer _tokenizer;
	private readonly InjectionContainer _injector;
    private HashSet<string> _silencedInputs;
    
	public CommandExecutor(
		CommandParser commandParser,
		CommandRegistry registry,
		Tokenizer tokenizer,
		InjectionContainer injector = null)
	{
		_registry = registry;
		_tokenizer = tokenizer;
		_commandParser = commandParser;
		_injector = injector;
	}

    public void SetSilencedInputs(HashSet<string> silencedInputs)
    {
        _silencedInputs = silencedInputs;
    }

    public void Execute(string input)
	{
		CommandParseResult result = _commandParser.Parse(input);

		if (!result.IsMatch)
		{
            if (_silencedInputs == null || !_silencedInputs.Contains(input.Trim()))
            {
                Debug.LogError(result.GetError());
            }
			return;
		}

		if (result.Entry == null)
		{
			return;
		}

		CommandEntry entry = result.Entry.Value;
		object[] parsedArgs = result.ArgTokens.Select(t => _tokenizer.ParseValue(t)).ToArray();

		Target? target = null;

		if (entry.HasTarget)
		{
			if (parsedArgs.Length > 0 && parsedArgs[0] is Target t)
			{
				target = t;
				parsedArgs = parsedArgs[1..];
			}
			else
			{
				Debug.LogError(
					$"Command '{entry.Method.Name}' has [TerminalTarget] but no target was parsed. Aborting."
				);
				return;
			}
		}

		object[] args = BuildArgs(entry.Method, parsedArgs);

		if (entry.IsStatic)
		{
			try
			{
				entry.Method.Invoke(null, args);
			}
			catch (TargetInvocationException ex)
			{
				Debug.LogError($"Command '{entry.Method.Name}' threw an exception: {ex.InnerException}");
			}
			return;
		}

		Component[] instances = _registry.GetInstances(entry.MonoType);
		bool stale = instances.Any(c => c == null);
		if (stale)
		{
			_registry.InvalidateInstanceCache(entry.MonoType);
			instances = _registry.GetInstances(entry.MonoType);
		}

		Component[] filtered = instances;
		if (target.HasValue && target.Value.Value != "@all")
		{
			string targetName = target.Value.Value[1..];
			filtered = instances.Where(c => c != null && c.gameObject.name == targetName).ToArray();
		}

		if (filtered.Length == 0)
		{
			string msg = target.HasValue
				? $"No active instance of '{entry.MonoType.Name}' found matching target '{target.Value.Value}'."
				: $"No active instance of '{entry.MonoType.Name}' found in the scene.";
			Debug.LogError(msg);
			return;
		}

		foreach (Component instance in filtered)
		{
			try
			{
				entry.Method.Invoke(instance, args);
			}
			catch (TargetInvocationException ex)
			{
				Debug.LogError($"Command '{entry.Method.Name}' on '{instance.gameObject.name}' threw an exception: {ex.InnerException}");
			}
		}
	}

	private object[] BuildArgs(MethodInfo method, object[] parsedArgs)
	{
		if (_injector == null)
		{
			return parsedArgs;
		}

		ParameterInfo[] parameters = method.GetParameters();
		var result = new List<object>();
		var parsedIndex = 0;

		foreach (ParameterInfo param in parameters)
		{
			if (param.GetCustomAttribute<InjectAttribute>() != null &&
				_injector.TryResolve(param.ParameterType, out object injected))
			{
				result.Add(injected);
			}
			else
			{
				result.Add(parsedIndex < parsedArgs.Length ? parsedArgs[parsedIndex] : null);
				parsedIndex++;
			}
		}

		return result.ToArray();
	}
}
}