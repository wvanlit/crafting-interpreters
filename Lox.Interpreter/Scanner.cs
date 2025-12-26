using Microsoft.Extensions.Logging;

namespace Lox.Interpreter;

public class Scanner(ILogger<Scanner> Logger)
{
    private string source = string.Empty;

    // String slice tracking
    private int start = 0;
    private int current = 0;

    // Human-friendly position tracking
    private int line = 1;
    private int positionInLine = 0;

    public IEnumerable<Token> ScanTokens(string source)
    {
        this.source = source;

        Logger.LogTrace("Scanning source");

        while (!IsAtEnd)
        {
            var c = Advance();

            switch (c)
            {
                // Single character tokens
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

                // 1 or 2 character tokens
                case '!': yield return Match('=') ? new BangEqualToken(CurrentPosition) : new BangToken(CurrentPosition); break;
                case '=': yield return Match('=') ? new EqualEqualToken(CurrentPosition) : new EqualToken(CurrentPosition); break;
                case '<': yield return Match('=') ? new LessEqualToken(CurrentPosition) : new LessToken(CurrentPosition); break;
                case '>': yield return Match('=') ? new GreaterEqualToken(CurrentPosition) : new GreaterToken(CurrentPosition); break;

                // Comments
                case '/':
                    if (Match('/'))
                    {
                        AdvanceUntilEndOfLine();
                    }
                    else
                    {
                        yield return new SlashToken(CurrentPosition);
                    }
                    break;

                // Whitespace
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    line++;
                    positionInLine = 0;
                    break;

                // Literals
                case '"': yield return ScanString(); break;
                default:
                    if (char.IsDigit(c))
                    {
                        yield return ScanNumber(); break;
                    }

                    if (char.IsLetter(c))
                    {
                        yield return ScanIdentifierOrKeyword(); break;
                    }


                    Logger.LogError("Illegal scan char of '{char}' at {position}", c, CurrentPosition);
                    break;
            }

            start = current;
        }

        Logger.LogTrace("Completed scanning source");
    }

    private StringToken ScanString()
    {
        var startingAt = CurrentPosition;

        while (Peek() != '\"' && !IsAtEnd)
        {
            var c = Advance();
            if (c == '\n')
            {
                line++;
                positionInLine = 0;
            }
        }

        if (IsAtEnd)
        {
            Logger.LogError("String at {position} is never closed", startingAt);
        }

        Advance(); // The closing "

        var literal = source.Substring(start + 1, current - 2);

        return new StringToken(Lexeme, literal, CurrentPosition);
    }

    private NumberToken ScanNumber()
    {
        while (char.IsDigit(Peek())) { Advance(); }

        if (Peek() == '.' && char.IsDigit(Peek(offset: 1)))
        {
            Advance(); // Consume the '.'

            while (char.IsDigit(Peek())) { Advance(); }
        }

        return new NumberToken(Lexeme, decimal.Parse(Lexeme), CurrentPosition);
    }

    private Token ScanIdentifierOrKeyword()
    {
        while (char.IsLetterOrDigit(Peek())) { Advance(); }

        return Lexeme.ToLowerInvariant() switch
        {
            "and" => new AndToken(CurrentPosition),
            "class" => new ClassToken(CurrentPosition),
            "else" => new ElseToken(CurrentPosition),
            "false" => new FalseToken(CurrentPosition),
            "fun" => new FunToken(CurrentPosition),
            "for" => new ForToken(CurrentPosition),
            "if" => new IfToken(CurrentPosition),
            "nil" => new NilToken(CurrentPosition),
            "or" => new OrToken(CurrentPosition),
            "print" => new PrintToken(CurrentPosition),
            "return" => new ReturnToken(CurrentPosition),
            "super" => new SuperToken(CurrentPosition),
            "this" => new ThisToken(CurrentPosition),
            "true" => new TrueToken(CurrentPosition),
            "var" => new VarToken(CurrentPosition),
            "while" => new WhileToken(CurrentPosition),
            _ => new IdentifierToken(Lexeme, CurrentPosition)
        };
    }

    private char Advance()
    {
        positionInLine++;
        return source.ElementAt(current++);
    }

    private bool Match(char expected)
    {
        if (IsAtEnd) return false;
        if (source.ElementAt(current) != expected) return false;

        current++;
        positionInLine++;

        return true;
    }

    private char Peek(int offset = 0) => current + offset >= source.Length ? '\0' : source.ElementAt(current + offset);

    private string AdvanceUntilEndOfLine()
    {
        var str = "";

        while (Peek() != '\n' && !IsAtEnd)
        {
            str += Advance();
        }

        return str;
    }

    private bool IsAtEnd => current >= source.Length;
    private TokenPosition CurrentPosition => (line, positionInLine);
    private string Lexeme => source.Substring(start, current);
}
