using Microsoft.Extensions.Logging;

namespace Lox.Interpreter;

public class Lox(
    ILogger<Lox> Logger,
    Scanner Scanner)
{
    public Task Evaluate(string code)
    {
        Logger.LogInformation("Running Lox interpreter");

        IEnumerable<Token> tokens = Scanner.ScanTokens(code);

        foreach (Token token in tokens)
        {
            Logger.LogInformation("Token: {Token}", token);
        }

        return Task.CompletedTask;
    }
}
