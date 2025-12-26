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
        var source = "(){},.-+;*";

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
            t => Assert.IsType<StarToken>(t)
        );
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

        logger.Errors.Template("Illegal scan char of '{char}' at {position}").With("char", '@').With("position", (1, 0));
        logger.Errors.Template("Illegal scan char of '{char}' at {position}").With("char", '#').With("position", (1, 1));
        logger.Errors.Template("Illegal scan char of '{char}' at {position}").With("char", '^').With("position", (1, 2));
    }
}
