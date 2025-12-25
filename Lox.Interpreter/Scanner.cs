using Microsoft.Extensions.Logging;

namespace Lox.Interpreter;

public class Scanner(ILogger<Scanner> Logger)
{
    private string source = string.Empty;
    private int start = 0;
    private int current = 0;
    private int line = 1;

    public IEnumerable<Token> ScanTokens(string source)
    {
        this.source = source;

        Logger.LogTrace("Scanning source");

        while (!IsAtEnd())
        {
            var c = Advance();
            switch (c)
            {
                case '(': yield return new LeftParenthesisToken(CurrentPosition()); break;
                case ')': yield return new RightParenthesisToken(CurrentPosition()); break;
                default:
                    throw new InvalidOperationException($"Illegal scan char of '{c}' at {CurrentPosition()}");
            }
        }
    }

    private bool IsAtEnd() => current >= source.Length;

    private char Advance() => source.ElementAt(current++);
    private TokenPosition CurrentPosition() => (line, start);
}
