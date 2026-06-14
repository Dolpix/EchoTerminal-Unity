using System;

namespace EchoTerminal.Scripts.TerminalCore.Token.TokenParser
{
public class LongParser : ITokenParser, IHintLabeler
{
	public Type Type => typeof(long);
	public string HintLabel => "long";

	public TokenState ParseTokenState(string raw, Type expectedType = null)
	{
		return long.TryParse(raw, out _) ? TokenState.Completed : TokenState.Failed;
	}

	public object ParseValue(string raw, Type expectedType = null)
	{
		return long.Parse(raw);
	}
}
}