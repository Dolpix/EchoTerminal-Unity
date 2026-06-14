using System;

namespace EchoTerminal.Scripts.TerminalCore.Token.TokenParser
{
public class IntParser : ITokenParser, IHintLabeler
{
	public Type Type => typeof(int);
	public string HintLabel => "int";

	public TokenState ParseTokenState(string raw, Type expectedType = null)
	{
		return int.TryParse(raw, out _) ? TokenState.Completed : TokenState.Failed;
	}

	public object ParseValue(string raw, Type expectedType = null)
	{
		return int.Parse(raw);
	}
}
}