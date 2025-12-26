namespace Lox.Interpreter.Tests;

public class ScannerTests
{
    private readonly TestLogger<Scanner> logger;
    private readonly Scanner sut;

    public ScannerTests()
    {
        logger = new TestLogger<Scanner>();
        sut = new Scanner(logger);
    }

    [Fact]
    public void GivenSingleCharacterTokens_WhenScanned_ThenCorrectTokensReturned()
    {
        // Arrange
        var source = "(){},.-+;*/";

        // Act
        var tokens = sut.ScanTokens(source).ToList();

        // Assert
        Assert.Collection(tokens,
            t => Assert.IsType<LeftParenthesisToken>(t),
            t => Assert.IsType<RightParenthesisToken>(t),
            t => Assert.IsType<LeftBraceToken>(t),
            t => Assert.IsType<RightBraceToken>(t),
            t => Assert.IsType<CommaToken>(t),
            t => Assert.IsType<DotToken>(t),
            t => Assert.IsType<MinusToken>(t),
            t => Assert.IsType<PlusToken>(t),
            t => Assert.IsType<SemicolonToken>(t),
            t => Assert.IsType<StarToken>(t),
            t => Assert.IsType<SlashToken>(t)
        );

        logger.Errors.None();
    }

    [Theory]
    [InlineData("! !=", new Type[] { typeof(BangToken), typeof(BangEqualToken) })]
    [InlineData("= ==", new Type[] { typeof(EqualToken), typeof(EqualEqualToken) })]
    [InlineData("< <=", new Type[] { typeof(LessToken), typeof(LessEqualToken) })]
    [InlineData("> >=", new Type[] { typeof(GreaterToken), typeof(GreaterEqualToken) })]
    public void GivenOneOrTwoCharacterTokens_WhenScanned_ThenCorrectTokensReturned(string source, Type[] expectedTokenTypes)
    {
        // Act
        var tokens = sut.ScanTokens(source).ToList();

        // Assert
        Assert.Equal(expectedTokenTypes.Length, tokens.Count);

        for (int i = 0; i < expectedTokenTypes.Length; i++)
        {
            Assert.IsType(expectedTokenTypes[i], tokens[i]);
        }

        logger.Errors.None();
    }

    [Fact]
    public void GivenNewlines_WhenScanned_ThenPositionsAreCorrect()
    {
        // Arrange
        var source = """
        ()

        {}
        """;

        // Act
        var tokens = sut.ScanTokens(source).ToList();

        // Assert
        Assert.Collection(tokens,
            t => Assert.Equal((1, 1), t.Position),
            t => Assert.Equal((1, 2), t.Position),
            t => Assert.Equal((3, 1), t.Position),
            t => Assert.Equal((3, 2), t.Position)
        );

        logger.Errors.None();
    }

    [Fact]
    public void GivenComments_WhenScanned_ThenCommentsAreIgnored()
    {
        // Arrange
        var source = @"
            // This is a comment
            ()
            // Another comment
            {}
        ";

        // Act
        var tokens = sut.ScanTokens(source).ToList();

        // Assert
        Assert.Collection(tokens,
            t => Assert.IsType<LeftParenthesisToken>(t),
            t => Assert.IsType<RightParenthesisToken>(t),
            t => Assert.IsType<LeftBraceToken>(t),
            t => Assert.IsType<RightBraceToken>(t)
        );

        logger.Errors.None();
    }

    [Fact]
    public void GivenIllegalCharacters_WhenScanned_ThenLogsErrorsAndSkipsTokens()
    {
        // Arrange
        var source = "@#^";

        // Act
        var tokens = sut.ScanTokens(source).ToList();

        // Assert
        Assert.Empty(tokens);

        logger.Errors.Count(3).AllMessagesContain("Illegal scan char");

        logger.Errors.Template("Illegal scan char of '{char}' at {position}").With("char", '@').With("position", (1, 1));
        logger.Errors.Template("Illegal scan char of '{char}' at {position}").With("char", '#').With("position", (1, 2));
        logger.Errors.Template("Illegal scan char of '{char}' at {position}").With("char", '^').With("position", (1, 3));
    }

    [Fact]
    public void GivenStringLiteral_WhenScanned_ThenStringTokenIncludesLexemeLiteralAndPosition()
    {
        // Arrange
        var source = "\"hi\"";

        // Act
        var tokens = sut.ScanTokens(source).ToList();

        // Assert
        var token = Assert.Single(tokens);
        var stringToken = Assert.IsType<StringToken>(token);
        Assert.Equal("\"hi\"", stringToken.Lexeme);
        Assert.Equal("hi", stringToken.Value);
        Assert.Equal((1, 4), stringToken.Position);

        logger.Errors.None();
    }

    [Fact]
    public void GivenMultilineStringLiteral_WhenScanned_ThenTracksLineAndColumn()
    {
        // Arrange
        var source = "\"a\nb\"";

        // Act
        var tokens = sut.ScanTokens(source).ToList();

        // Assert
        var token = Assert.Single(tokens);
        var stringToken = Assert.IsType<StringToken>(token);
        Assert.Equal("\"a\nb\"", stringToken.Lexeme);
        Assert.Equal("a\nb", stringToken.Value);
        Assert.Equal((2, 2), stringToken.Position);

        logger.Errors.None();
    }

    [Fact]
    public void GivenIntegerLiteral_WhenScanned_ThenNumberTokenIncludesLexemeLiteralAndPosition()
    {
        // Arrange
        var source = "123";

        // Act
        var tokens = sut.ScanTokens(source).ToList();

        // Assert
        var token = Assert.Single(tokens);
        var numberToken = Assert.IsType<NumberToken>(token);
        Assert.Equal("123", numberToken.Lexeme);
        Assert.Equal(123m, numberToken.Value);
        Assert.Equal((1, 3), numberToken.Position);

        logger.Errors.None();
    }

    [Fact]
    public void GivenDecimalLiteral_WhenScanned_ThenNumberTokenIncludesLexemeLiteralAndPosition()
    {
        // Arrange
        var source = "12.34";

        // Act
        var tokens = sut.ScanTokens(source).ToList();

        // Assert
        var token = Assert.Single(tokens);
        var numberToken = Assert.IsType<NumberToken>(token);
        Assert.Equal("12.34", numberToken.Lexeme);
        Assert.Equal(12.34m, numberToken.Value);
        Assert.Equal((1, 5), numberToken.Position);

        logger.Errors.None();
    }

    [Fact]
    public void GivenNumberWithTrailingDot_WhenScanned_ThenDotIsSeparateToken()
    {
        // Arrange
        var source = "12.";

        // Act
        var tokens = sut.ScanTokens(source).ToList();

        // Assert
        Assert.Collection(tokens,
            t =>
            {
                var numberToken = Assert.IsType<NumberToken>(t);
                Assert.Equal("12", numberToken.Lexeme);
                Assert.Equal(12m, numberToken.Value);
                Assert.Equal((1, 2), numberToken.Position);
            },
            t =>
            {
                var dotToken = Assert.IsType<DotToken>(t);
                Assert.Equal((1, 3), dotToken.Position);
            }
        );

        logger.Errors.None();
    }

    [Theory]
    [InlineData("and", typeof(AndToken))]
    [InlineData("class", typeof(ClassToken))]
    [InlineData("else", typeof(ElseToken))]
    [InlineData("false", typeof(FalseToken))]
    [InlineData("fun", typeof(FunToken))]
    [InlineData("for", typeof(ForToken))]
    [InlineData("if", typeof(IfToken))]
    [InlineData("nil", typeof(NilToken))]
    [InlineData("or", typeof(OrToken))]
    [InlineData("print", typeof(PrintToken))]
    [InlineData("return", typeof(ReturnToken))]
    [InlineData("super", typeof(SuperToken))]
    [InlineData("this", typeof(ThisToken))]
    [InlineData("true", typeof(TrueToken))]
    [InlineData("var", typeof(VarToken))]
    [InlineData("while", typeof(WhileToken))]
    public void GivenKeywords_WhenScanned_ThenKeywordTokensReturned(string source, Type expectedTokenType)
    {
        // Act
        var tokens = sut.ScanTokens(source).ToList();

        // Assert
        var token = Assert.Single(tokens);
        Assert.IsType(expectedTokenType, token);

        logger.Errors.None();
    }

    [Fact]
    public void GivenIdentifier_WhenScanned_ThenIdentifierTokenReturned()
    {
        // Arrange
        var source = "exampleIdentifier123";

        // Act
        var tokens = sut.ScanTokens(source).ToList();

        // Assert
        var token = Assert.Single(tokens);
        var identifier = Assert.IsType<IdentifierToken>(token);
        Assert.Equal(source, identifier.Lexeme);

        logger.Errors.None();
    }
}
