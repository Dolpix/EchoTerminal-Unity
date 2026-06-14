using System;

namespace EchoTerminal.Scripts.TerminalCore.Token.TokenParser
{
public class DoubleParser : ITokenParser, IHintLabeler
{
	public Type Type => typeof(double);
	public string HintLabel => "double";

	public TokenState ParseTokenState(string raw, Type expectedType = null)
	{
		return double.TryParse(raw, out _) ? TokenState.Completed : TokenState.Failed;
	}

	public object ParseValue(string raw, Type expectedType = null)
	{
		return double.Parse(raw);
	}
}
}