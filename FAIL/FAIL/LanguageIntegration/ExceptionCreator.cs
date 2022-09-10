using FAIL.Exceptions;
using NotSupportedException = FAIL.Exceptions.NotSupportedException;
using Object = FAIL.ElementTree.Instance;

namespace FAIL.LanguageIntegration;
internal static class ExceptionCreator
{
    public static SyntaxException UnexpectedToken(Token token)
    {
        var message = $"Unexpected token '{token.Value}' at line {token.Row} and colum {token.Column}!";

        _ = Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(message, token.Row, token.Column, token.FileName);
    }

    public static SyntaxException InvalidToken(Token token, TokenType expected)
    {
        var message = $"Invalid token '{token.Value}' in line {token.Row} and column {token.Column}! '{expected}' expected.";

        _ = Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(message, token.Row, token.Column, token.FileName);
    }

    public static StopIterationException IterationEnded() => new();

    public static SyntaxException NotAChar(Token token)
    {
        var message = $"Expression '{token.Value}' is not a valid char!";

        _ = Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(message, token.Row, token.Column, token.FileName);
    }

    public static NotAssignedException NotAssignedInScope(Token token)
    {
        var message = $"'{token.Value}' is not assigned in the current scope!";

        _ = Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(token.Value, message, token.Row, token.Column, token.FileName);
    }

    public static AlreadyAssignedException AlreadyDeclaredInScope(Token token)
    {
        var message = $"'{token.Value}' is already defined in the current scope!";

        _ = Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(token.Value, message, token.Row, token.Column, token.FileName);
    }

    public static WrongTypeException VariableExpected() => new("", 0, 0, "");
    public static WrongTypeException BooleanExpected() => new("", 0, 0, "");

    public static WrongCountException WrongParameterCount(int expected, int given, string funcName, Token? token)
    {
        var message = $"No overload for function '{funcName}' takes {given} arguments!";

        _ = Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(expected, given, message, token?.Row ?? 0, token?.Column ?? 0, token?.FileName ?? "");
    }

    public static Exception FunctionMustReturnValue(string funcName)
    {
        var message = $"Function '{funcName}' must return a value, as its return type is not void!";

        _ = Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(message);
    }

    public static InvalidTypeException InvalidType(string name, ElementTree.Type given, ElementTree.Type expected, Token? token)
    {
        var message = $"'{name}' was given type '{given.Name}', when type '{expected.Name}' was expected!";

        _ = Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(name, given, expected, message, token?.Row ?? 0, token?.Column ?? 0, token?.FileName ?? "");
    }

    public static InvalidTypeException SpecificTypeNeeded(string functionName, Token token)
    {
        var message = "A specific type declaration is needed!";

        _ = Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(functionName, new("var"), new("Undefined"), message, token.Row, token.Column, token.FileName);
    }

    public static OverloadAlreadyExists OverloadAlreadyExists(string functionName)
    {
        var message = $"An overload with the same parameters already exists on function '{functionName}'";

        _ = Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(functionName, message, 0, 0, "");
    }

    public static NotAssignedException OverloadNotFound(string functionName)
    {
        var message = $"No matching overload was found on function '{functionName}'!";

        _ = Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(functionName, message, 0, 0, "");
    }

    public static NotAssignedException UseOfUnassignedVariable(string variableName, Token? token)
    {
        var message = $"Use of unassigned variable '{variableName}'!";

        _ = Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(variableName, message, token?.Row ?? 0, token?.Column ?? 0, token?.FileName ?? "");
    }

    public static NotSupportedException BinaryOperationNotSupported(Token operatorToken, ElementTree.Type first, ElementTree.Type second)
    {
        var message = $"Operator '{operatorToken.Value}' is not supported for types '{first}' and '{second}'!";

        _ = Interpreter.Logger!.Log(message, LogLevel.Error);
        return new(message, operatorToken.Row, operatorToken.Column, operatorToken.FileName);
    }

    public static NotSupportedException UnaryOperationNotSupported(Token operatorToken, ElementTree.Type type)
    {
        var message = $"Operator '{operatorToken.Value}' is not supported for type '{type}'!";

        _ = Interpreter.Logger!.Log(message, LogLevel.Error);
        return new(message, operatorToken.Row, operatorToken.Column, operatorToken.FileName);
    }
    public static NotSupportedException ExplicitConversionNotSupported(Token operatorToken, ElementTree.Type newType, ElementTree.Type oldType)
    {
        var message = $"Conversion to type '{newType}' is not supported for type '{oldType}'!";

        _ = Interpreter.Logger!.Log(message, LogLevel.Error);
        return new(message, operatorToken.Row, operatorToken.Column, operatorToken.FileName);
    }
}
