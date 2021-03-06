using FAIL.Exceptions;

namespace FAIL.LanguageIntegration;
internal static class ExceptionCreator
{
    public static SyntaxException UnexpectedToken(Token token)
    {
        var message = $"Unexpected token '{token.Value}' at line {token.Row} and colum {token.Column}!";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(message, token.Row, token.Column, token.FileName);
    }

    public static SyntaxException WrongToken(Token token, TokenType expected)
    {
        var message = $"Invalid token '{token.Value}' in line {token.Row} and column {token.Column}! '{expected}' expected.";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(message, token.Row, token.Column, token.FileName);
    }

    public static StopIterationException IterationEnded() => new();

    public static SyntaxException NotAChar(Token token)
    {
        var message = $"Expression {token.Value} is not a valid char!";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(message, token.Row, token.Column, token.FileName);
    }

    public static NotAssignedException NotAssignedInScope(Token token)
    {
        var message = $"'{token.Value}' is not assigned in the current scope!";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(token.Value, message, token.Row, token.Column, token.FileName);
    }

    public static AlreadyAssignedException AlreadyAssignedInScope(Token token)
    {
        var message = $"'{token.Value}' is already defined in the current scope!";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(token.Value, message, token.Row, token.Column, token.FileName);
    }

    public static WrongTypeException VariableExpected() => new("", 0, 0, "");
    public static WrongTypeException BooleanExpected() => new("", 0, 0, "");

    public static WrongCountException WrongParameterCount(int expected, int given, string funcName, Token? token)
    {
        var message = $"No overload for function '{funcName}' takes {given} arguments!";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(expected, given, message, token?.Row ?? 0, token?.Column ?? 0, token?.FileName ?? "");
    }

    public static Exception FunctionMustReturnValue(string funcName)
    {
        var message = $"Function '{funcName}' must return a value, as its return type is not void!";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(message);
    }

    public static InvalidTypeException InvalidType(string name, ElementTree.Type given, ElementTree.Type expected, Token? token)
    {
        var message = $"'{name}' was given type '{given.Name}', when type '{expected.Name}' was expected!";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(name, given, expected, message, token?.Row ?? 0, token?.Column ?? 0, token?.FileName ?? "");
    }

    public static InvalidTypeException SpecificTypeNeeded(string functionName, Token token)
    {
        var message = "A specific type declaration is needed!";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(functionName, new("var"), new("Undefined"), message, token.Row, token.Column, token.FileName);
    }

    public static OverloadAlreadyExists OverloadAlreadyExists(string functionName)
    {
        var message = $"An overload with the same parameters already exists on function '{functionName}'";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(functionName, message, 0, 0, "");
    }

    public static NotAssignedException OverloadNotFound(string functionName)
    {
        var message = $"No matching overload was found on function '{functionName}'!";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(functionName, message, 0, 0, "");
    }

    public static NotAssignedException UseOfUnassignedVariable(string variableName, Token? token)
    {
        var message = $"Use of unassigned variable '{variableName}'";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(variableName, message, token?.Row ?? 0, token?.Column ?? 0, token?.FileName ?? "");
    }
}
