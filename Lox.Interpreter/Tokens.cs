namespace Lox.Interpreter;

/// <param name="Lexeme">Raw substring of the source code</param>
/// <param name="Literal">The literal value represented by the token, if any</param>
/// <param name="Position">The position of the token in the source code</param>
public abstract record Token(string Lexeme, object Literal, TokenPosition Position);

/*
 *  Single-character tokens
 */

public record LeftParenthesisToken(TokenPosition Position) : Token("(", null, Position);
public record RightParenthesisToken(TokenPosition Position) : Token(")", null, Position);
public record LeftBraceToken(TokenPosition Position) : Token("{", null, Position);
public record RightBraceToken(TokenPosition Position) : Token("}", null, Position);
public record CommaToken(TokenPosition Position) : Token(",", null, Position);
public record DotToken(TokenPosition Position) : Token(".", null, Position);
public record MinusToken(TokenPosition Position) : Token("-", null, Position);
public record PlusToken(TokenPosition Position) : Token("+", null, Position);
public record SemicolonToken(TokenPosition Position) : Token(";", null, Position);
public record SlashToken(TokenPosition Position) : Token("/", null, Position);
public record StarToken(TokenPosition Position) : Token("*", null, Position);

/*
 *  One or two character tokens
 */

public record BangToken(TokenPosition Position) : Token("!", null, Position);
public record BangEqualToken(TokenPosition Position) : Token("!=", null, Position);
public record EqualToken(TokenPosition Position) : Token("=", null, Position);
public record EqualEqualToken(TokenPosition Position) : Token("==", null, Position);
public record GreaterToken(TokenPosition Position) : Token(">", null, Position);
public record GreaterEqualToken(TokenPosition Position) : Token(">=", null, Position);
public record LessToken(TokenPosition Position) : Token("<", null, Position);
public record LessEqualToken(TokenPosition Position) : Token("<=", null, Position);

/*
 *  Literals
 */

public record IdentifierToken(string Lexeme, TokenPosition Position) : Token(Lexeme, null, Position);
public record StringToken(string Lexeme, string Value, TokenPosition Position) : Token(Lexeme, Value, Position);
public record NumberToken(string Lexeme, decimal Value, TokenPosition Position) : Token(Lexeme, Value, Position);

/*
 *  Keywords
 */

public record AndToken(TokenPosition Position) : Token("and", null, Position);
public record ClassToken(TokenPosition Position) : Token("class", null, Position);
public record ElseToken(TokenPosition Position) : Token("else", null, Position);
public record FalseToken(TokenPosition Position) : Token("false", null, Position);
public record FunToken(TokenPosition Position) : Token("fun", null, Position);
public record ForToken(TokenPosition Position) : Token("for", null, Position);
public record IfToken(TokenPosition Position) : Token("if", null, Position);
public record NilToken(TokenPosition Position) : Token("nil", null, Position);
public record OrToken(TokenPosition Position) : Token("or", null, Position);
public record PrintToken(TokenPosition Position) : Token("print", null, Position);
public record ReturnToken(TokenPosition Position) : Token("return", null, Position);
public record SuperToken(TokenPosition Position) : Token("super", null, Position);
public record ThisToken(TokenPosition Position) : Token("this", null, Position);
public record TrueToken(TokenPosition Position) : Token("true", null, Position);
public record VarToken(TokenPosition Position) : Token("var", null, Position);
public record WhileToken(TokenPosition Position) : Token("while", null, Position);

/*
 *  Special
 */

public record EndOfFileToken(TokenPosition Position) : Token(string.Empty, null, Position);
