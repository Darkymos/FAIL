using FAIL.Exceptions;

namespace FAIL.Language_Integration;
internal static class ExceptionCreator
{
    public static SyntaxException UnexpectedToken(Token token)
    {
        var message = $"Unexpected token '{token.Value}' at line {token.Row} and colum {token.Column}!";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(message, token.FileName);
    }

    public static SyntaxException WrongToken(Token token, TokenType expected)
    {
        var message = $"Invalid token '{token.Value}' in line {token.Row} and column {token.Column}! '{expected}' expected.";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(message, token.FileName);
    }

    public static StopIterationException IterationEnded() => new();
}
