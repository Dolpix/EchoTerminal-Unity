using System;

namespace EchoTerminal.Scripts.TerminalCore.Token.TokenParser
{
public class FloatParser : ITokenParser, IHintLabeler
{
	public Type Type => typeof(float);
	public string HintLabel => "float";

	public TokenState ParseTokenState(string raw, Type expectedType = null)
	{
		return float.TryParse(raw, out _) ? TokenState.Completed : TokenState.Failed;
	}

	public object ParseValue(string raw, Type expectedType = null)
	{
		return float.Parse(raw);
	}
}
}