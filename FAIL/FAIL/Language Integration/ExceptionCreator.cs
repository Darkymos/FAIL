﻿using FAIL.Exceptions;

namespace FAIL.LanguageIntegration;
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

    public static SyntaxException NotAChar(string value, string fileName)
    {
        var message = $"Expression {value} is not a valid char!";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(message, fileName);
    }

    public static NotAssignedException NotAssignedInScope(string name)
    {
        var message = $"'{name}' is not assigned in the current scope!";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(name, message);
    }

    public static AlreadyAssignedException AlreadyAssignedInScope(string name)
    {
        var message = $"'{name}' is already defined in the current scope!";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(name, message);
    }

    public static WrongTypeException VariableExpected() => new();
    public static WrongTypeException BooleanExpected() => new();

    public static WrongCountException WrongParameterCount(int expected, int given, string funcName)
    {
        var message = $"No overload for function '{funcName}' takes {given} arguments!";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(expected, given, message);
    }

    public static Exception FunctionMustReturnValue(string funcName)
    {
        var message = $"Function '{funcName}' must return a value, as its return type is not void!";

        Interpreter.Logger!.Log(message, LogLevel.Critical);
        return new(message);
    }
}
