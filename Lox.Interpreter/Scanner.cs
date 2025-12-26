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
                case '(': yield return new LeftParenthesisToken(CurrentPosition); break;
                case ')': yield return new RightParenthesisToken(CurrentPosition); break;
                case '{': yield return new LeftBraceToken(CurrentPosition); break;
                case '}': yield return new RightBraceToken(CurrentPosition); break;
                case ',': yield return new CommaToken(CurrentPosition); break;
                case '.': yield return new DotToken(CurrentPosition); break;
                case '-': yield return new MinusToken(CurrentPosition); break;
                case '+': yield return new PlusToken(CurrentPosition); break;
                case ';': yield return new SemicolonToken(CurrentPosition); break;
                case '*': yield return new StarToken(CurrentPosition); break;
                default:
                    Logger.LogError("Illegal scan char of '{char}' at {position}", c, CurrentPosition);
                    break;
            }

            start = current;
        }
    }

    private bool IsAtEnd() => current >= source.Length;

    private char Advance() => source.ElementAt(current++);
    private TokenPosition CurrentPosition => (line, start);
}
