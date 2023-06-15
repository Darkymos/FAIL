﻿using FAIL.BuiltIn.DataTypes;
using FAIL.ElementTree;
using FAIL.Helpers;
using FAIL.Metadata;
using static FAIL.BuiltIn.BuiltInFunctions;

namespace FAIL.LanguageIntegration;
public sealed class Parser : IParser
{
    private readonly List<Token> Tokens;
    private Token CurrentToken => Tokens.First();

    // they just map tokens to types of the element tree
    // kinda redundant, but i haven't found a way to replace them yet
    private static readonly Dictionary<string, BinaryOperation> DotOperatorMapper = new()
    {
        { "*", BinaryOperation.Multiplication },
        { "/", BinaryOperation.Division },
    };
    private static readonly Dictionary<string, BinaryOperation> StrokeOperatorMapper = new()
    {
        { "+", BinaryOperation.Addition },
        { "-", BinaryOperation.Substraction },
    };
    private static readonly Dictionary<string, BinaryOperation> TestOperatorMapper = new()
    {
        { "==", BinaryOperation.Equal },
        { "!=", BinaryOperation.NotEqual },
        { ">=", BinaryOperation.GreaterThanOrEqual },
        { "<=", BinaryOperation.LessThanOrEqual },
        { ">", BinaryOperation.GreaterThan },
        { "<", BinaryOperation.LessThan },
    };
    private static readonly Dictionary<string, BinaryOperation> LogicalOperatorMapper = new()
    {
        { "or", BinaryOperation.Or },
        { "and", BinaryOperation.And },
        { "||", BinaryOperation.Or },
        { "&&", BinaryOperation.And },
    };
    private static readonly Dictionary<string, BinaryOperation> SelfAssignmentOperatorMapper = new()
    {
        { "+=", BinaryOperation.Addition },
        { "-=", BinaryOperation.Substraction },
        { "*=", BinaryOperation.Multiplication },
        { "/=", BinaryOperation.Division },
    };
    private static readonly Dictionary<string, BinaryOperation> IncrementalOperatorMapper = new()
    {
        { "++", BinaryOperation.Addition },
        { "--", BinaryOperation.Substraction },
    };
    private static Dictionary<string, BinaryOperation> GetOperationDictionary(Calculations calculation) => calculation switch
    {
        Calculations.DotCalculations => DotOperatorMapper,
        Calculations.StrokeCalculations => StrokeOperatorMapper,
        Calculations.TestOperations => TestOperatorMapper,
        Calculations.LogicalOperations => LogicalOperatorMapper,
        _ => throw new NotSupportedException()
    };

    public Parser(ITokenizer tokenizer) => Tokens = tokenizer.Tokenize();


    public CommandList Parse() => Tokens.Any()
        ? ParseCommandList(TokenType.EndOfStatement)
            .ExecuteIf(x => Tokens.Any(), x => throw ExceptionCreator.UnexpectedToken(CurrentToken))
        : new();

    private CommandList ParseCommandList(TokenType endOfStatementSign, TokenType? endOfBlockSign = null, params Scope[] shared)
        => new(new Scope(new(), shared)
        .AddWhile(() => Tokens.Any() && (endOfBlockSign is null || CurrentToken.Type != endOfBlockSign.Value), x => ParseCommand(x, endOfStatementSign, endOfBlockSign))
        .Expect(x => endOfBlockSign is null || CurrentToken.Type == endOfBlockSign.Value,
            failure: x => throw ExceptionCreator.UnexpectedToken(x.Last().Token!.Value))
        .ExecuteIf(x => endOfBlockSign is not null, x => Tokens.AcceptExpected(endOfBlockSign!.Value)));

    private AST ParseCommand(Scope scope, TokenType? endOfStatementSign = null, TokenType? endOfBlockSign = null)
    {
        var isBlock = false; // kinda redundant, but still there, until a better solution is found

        return (CurrentToken.Type switch
        {
            TokenType.Var or TokenType.Void or TokenType.DataType => ParseType(scope).Then(x => isBlock = x is Function),
            TokenType.Class => ParseClass(scope).Then(_ => isBlock = true),
            TokenType.New => ParseTypeInitialization(scope),
            TokenType.If => ParseIf(scope, Tokens.ConsumeAndGet()).Then(_ => isBlock = true),
            TokenType.While => ParseWhile(scope, Tokens.ConsumeAndGet()).Then(_ => isBlock = true),
            TokenType.For => ParseFor(scope, Tokens.ConsumeAndGet()).Then(_ => isBlock = true),
            TokenType.Return => Tokens.ConsumeAndGet().ThenTo(x => new Return(ParseCommand(scope, endOfStatementSign), x)),
            TokenType.Continue => new Continue(Tokens.ConsumeAndGet()),
            TokenType.Break => new Break(Tokens.ConsumeAndGet()),
            _ => ParseArithmetic(scope)
        })
        .ExecuteIf(x => !(endOfStatementSign is null || isBlock || (endOfBlockSign is not null && CurrentToken.Type == endOfBlockSign.Value)),
                   x => Tokens.AcceptExpected(endOfStatementSign!.Value));
    }

    private AST ParseArithmetic(Scope scope, AST? heap = null) => ParseArithmetic(scope, CalculationsExtensions.All, heap);
    private AST ParseArithmetic(Scope scope, Calculations calculations, AST? heap = null)
    {
        if (calculations.HasFlag(Calculations.Term)) heap = ParseTerm(scope, heap);
        if (calculations.HasFlag(Calculations.DotCalculations)) heap = ParseArithmeticOperation(scope, heap, Calculations.DotCalculations);
        if (calculations.HasFlag(Calculations.StrokeCalculations)) heap = ParseArithmeticOperation(scope, heap, Calculations.StrokeCalculations);
        if (calculations.HasFlag(Calculations.TestOperations)) heap = ParseArithmeticOperation(scope, heap, Calculations.TestOperations);
        if (calculations.HasFlag(Calculations.LogicalOperations)) heap = ParseArithmeticOperation(scope, heap, Calculations.LogicalOperations);
        if (calculations.HasFlag(Calculations.Conversions)) heap = ParseConversion(heap);

        return heap!;
    }
    private AST? ParseTerm(Scope scope, AST? heap)
    {
        return CurrentToken.Type switch
        {
            TokenType.Identifier => Tokens.GetCurrent(out var token).Consume().GetCurrent().Type switch
            {
                TokenType.Assignment => ParseAssignment(scope, token),
                TokenType.SelfAssignment => ParseSelfAssignment(scope, token),
                TokenType.OpeningParenthesis => ParseFunctionCall(scope, token),
                TokenType.IncrementalOperator => ParseIncrementalOperator(scope, token),
                TokenType.Accessor => ParseTypeMember(scope, token),
                _ => new Reference(Parser.GetValidVariable(scope, token.Value, token), scope, token)
            },
            TokenType.OpeningParenthesis => Tokens.AcceptExpected(TokenType.OpeningParenthesis).ThenTo(_ => ParseArithmetic(scope)).Then(_ => Tokens.AcceptExpected(TokenType.ClosingParenthesis)),
            TokenType.StrokeCalculation => Tokens.AcceptAndGetExpected(TokenType.StrokeCalculation).ThenTo(x => x.Value is "+"
                ? ParseArithmetic(scope, ParseArithmetic(scope, Calculations.Term))
                : ParseArithmetic(scope, new UnaryOperator(UnaryOperation.Negation, ParseArithmetic(scope, Calculations.Term)))),
            TokenType.WriteLine => Tokens.Consume().ThenTo(_ => ParseArithmetic(scope)).ThenTo(x => new BuiltInFunctionCall("|>", new(new Scope { x }))),
            _ => null
        } is not null and AST result
            ? result
        : CurrentToken.Type is TokenType.LogicalOperator && (CurrentToken.Value is "not" or "!")
            ? Tokens.AcceptExpected(TokenType.LogicalOperator).ThenTo(_ => ParseArithmetic(scope, new UnaryOperator(UnaryOperation.Not, ParseArithmetic(scope, Calculations.Term))))
        : CurrentToken.Type is (TokenType.String or TokenType.Char or TokenType.Boolean or TokenType.Integer or TokenType.Double) and TokenType type
            ? Tokens.ConsumeAndGet().ThenTo(x => new Instance(new ElementTree.Type(type.ToString()), x.Value, x))
        : heap;
    }
    private AST? ParseArithmeticOperation(Scope scope, AST? heap, Calculations calculation)
    {
        return Tokens.Any() && CurrentToken.Type == calculation.GetOperationTokenType()
            ? Tokens.ConsumeAndGet()
                    .ThenTo(x => ParseArithmetic(scope, calculation.SelfAndBelow(), new BinaryOperator(GetOperationDictionary(calculation)[x.Value], heap, ParseArithmetic(scope, calculation.Above()), x)))
            : heap;
    }
    private AST? ParseConversion(AST? heap) => Tokens.Any() && CurrentToken.Type is TokenType.Conversion
        ? Tokens.ConsumeAndGet().ThenTo(x => Tokens.ConsumeAndGetExpected(x => x.Type is TokenType.DataType, x => throw ExceptionCreator.InvalidToken(x, TokenType.DataType))
            .ThenTo(x => new TypeConversion(heap!, new ElementTree.Type(x.Value), x)))
        : heap;

    private AST ParseType(Scope scope) => Tokens.GetCurrent(out var type).Consume().GetCurrent(out var identifier)
        .AcceptExpected(TokenType.Identifier).GetCurrent().Type is TokenType.OpeningParenthesis
            ? ParseFunction(scope, type, identifier)
            : ParseVar(scope, type, identifier);
    private AST ParseVar(Scope scope, Token type, Token identifier)
        => identifier.ExecuteIfAndGet(x => IsIdentifierUnique(scope, x.Value),
            x => CurrentToken.Type,
            x => throw ExceptionCreator.AlreadyDeclaredInScope(x))
        .ExecuteIfAndGet(x => x is not TokenType.Assignment,
            _ => new Variable(identifier.Value, new ElementTree.Type(type.Value), null, token: identifier),
            _ => Tokens.AcceptExpected(TokenType.Assignment)
                .ThenTo(_ => new Variable(identifier.Value, CheckType(ParseCommand(scope), new ElementTree.Type(type.Value), identifier.Value, identifier), identifier)));
    private AST ParseFunction(Scope scope, Token type, Token identifier)
    {
        // functions must declare specific types for their return value to avoid major issues with result types on calculations
        if (type.Type.ToString() == "var") throw ExceptionCreator.SpecificTypeNeeded(identifier.Value, identifier);

        // 'parameters' may be empty
        var parameters = Tokens.AcceptExpected(TokenType.OpeningParenthesis).ThenTo(_ => ParseCommandList(TokenType.Separator, TokenType.ClosingParenthesis));

        // functions must declare specific types for their parameters to avoid major issues with result types on calculations
        foreach (var parameter in parameters.Commands.Entries.Cast<Variable>().Where(parameter => parameter.Type.Name == "var"))
            throw ExceptionCreator.SpecificTypeNeeded(identifier.Value, parameter.Token!.Value);

        var body = ParseBody(parameters.Commands, scope);

        // if there is a return type declared in front of the identifier, there has to be a return a the end (currently)
        if (type.Type != TokenType.Void && body.Commands.Entries.Last() is not Return)
            throw ExceptionCreator.FunctionMustReturnValue(identifier.Value);

        // if there is an return type, we have to check it, functions without return types may return something, which just won't be validated
        if (type.Type != TokenType.Void) _ = CheckType(body.Commands.Entries.Last().GetType(), new ElementTree.Type(type.Value), "return", body.Commands.Entries.Last().Token!.Value);

        return ((Function)GetFunctionFromScope(scope, identifier.Value))!.ExecuteIfAndGet(x => x is not null,
            x => x.Then(x => x.AddOverload(new(new ElementTree.Type(type.Value), parameters, body))),
            x => new Function(identifier.Value, identifier).Then(x => x.AddOverload(new(new ElementTree.Type(type.Value), parameters, body))));
    }

    private AST ParseClass(Scope scope) => Tokens.AcceptExpected(TokenType.Class).AcceptAndGetExpected(TokenType.Identifier)
        .ExecuteIf(x => !IsIdentifierUnique(scope, x.Value), x => throw ExceptionCreator.AlreadyDeclaredInScope(x))
        .ThenTo(x => new CustomClass(x.Value, ParseBody(false, scope), x));
    private AST ParseTypeInitialization(Scope scope) => Tokens.AcceptExpected(TokenType.New).GetCurrent(out var typeName)
        .AcceptExpected(TokenType.Identifier).AcceptExpected(TokenType.OpeningParenthesis)
        .ThenTo(_ => ParseCommandList(TokenType.Separator, TokenType.ClosingParenthesis, scope))
        .ThenTo(x => new Instance(new ElementTree.Type(typeName.Value), scope, x));

    private AST ParseAssignment(Scope scope, Token token) => Tokens.AcceptExpected(TokenType.Assignment)
        .ThenTo(_ => (Variable)GetValidVariable(scope, token.Value, token)).Store(out var variable)
        .ThenTo(_ => ParseCommand(scope)).Then(x => CheckType(x.GetType(), variable.GetType(), variable.Name, token))
        .ThenTo(x => new Assignment(variable, x, token));
    private AST ParseSelfAssignment(Scope scope, Token token) => Tokens.GetCurrent(out var op).AcceptExpected(TokenType.SelfAssignment)
        .ThenTo(_ => (Variable)GetValidVariable(scope, token.Value, token)).Store(out var variable)
        .ThenTo(_ => ParseCommand(scope)).Then(x => CheckType(x.GetType(), variable.GetType(), variable.Name, token))
        .ThenTo(x => new Assignment(variable, new BinaryOperator(SelfAssignmentOperatorMapper[op.Value], variable, x, token)));
    private AST ParseFunctionCall(Scope scope, Token? token) => Tokens.AcceptExpected(TokenType.OpeningParenthesis)
        .ThenTo(_ => ParseCommandList(TokenType.Separator, TokenType.ClosingParenthesis, scope))
        .ThenTo(x => Functions.ContainsKey(token!.Value.Value)
            ? (AST)new BuiltInFunctionCall(token!.Value.Value, x)
            : new FunctionCall(GetFunctionFromScope(scope, token!.Value.Value), x, token));
    private AST ParseIncrementalOperator(Scope scope, Token token) => Tokens.GetCurrent(out var op).Consume()
        .ThenTo<List<Token>, Variable>(_ => GetValidVariable(scope, token.Value, token))
        .ThenTo(x => new Assignment(x, new BinaryOperator(IncrementalOperatorMapper[op.Value], x, new Instance(Integer.Type, 1), token)));
    private AST ParseTypeMember(Scope scope, Token token) => Tokens.AcceptExpected(TokenType.Accessor)
        .ThenTo(_ => new Reference(GetValidVariable(scope, token.Value, token), scope))
        .ThenTo(x => new InstanceCall(x, ParseArithmetic(x.Variable.Call()!.GetValueAs<CustomClass>().Members.Commands, Calculations.Term)));

    private AST ParseIf(Scope scope, Token token) => Tokens.AcceptExpected(TokenType.OpeningParenthesis)
        .ThenTo(_ => ParseCommand(scope, TokenType.ClosingParenthesis)).Store(out var testCommand)
        .ThenTo(_ => ParseBody(scope))
        .ExecuteIfAndGet(_ => Tokens.Any() && CurrentToken.Type is TokenType.Else,
            x => Tokens.Consume().ThenTo(_ => new If(testCommand!, x, CurrentToken.Type is TokenType.If ? ParseCommand(scope) : ParseBody(scope), token)),
            x => new If(testCommand!, x, null, token));
    private AST ParseWhile(Scope scope, Token token) => Tokens.AcceptExpected(TokenType.OpeningParenthesis)
        .ThenTo(_ => ParseCommand(scope, TokenType.ClosingParenthesis))
        .ThenTo(x => new While(x, ParseBody(scope), token));
    private AST ParseFor(Scope scope, Token token)
    {
        _ = Tokens.AcceptExpected(TokenType.OpeningParenthesis);

        var internalScope = new Scope(); // special scope for the iterator variable
        var iteratorVariable = ParseCommand(internalScope, TokenType.EndOfStatement); // var i = 0;
        internalScope.Add(iteratorVariable);
        var iteratorTest = ParseCommand(internalScope, TokenType.EndOfStatement); // i < length;
        var iteratorAction = ParseCommand(internalScope, TokenType.ClosingParenthesis); // i++

        return new For(iteratorVariable, iteratorTest, iteratorAction, ParseBody(internalScope, scope), token);
    }

    private CommandList ParseBody(params Scope[] scopes) => ParseBody(true, scopes);
    private CommandList ParseBody(bool allowSingleStatement, params Scope[] scopes)
    {
        if (CurrentToken.Type is TokenType.OpeningBracket)
        {
            _ = Tokens.AcceptExpected(TokenType.OpeningBracket);
            return ParseCommandList(TokenType.EndOfStatement, TokenType.ClosingBracket, scopes);
        }

        return allowSingleStatement
            ? new(new Scope(new List<AST>() { ParseCommand(new Scope(scopes)) }))
            : throw ExceptionCreator.InvalidToken(CurrentToken, TokenType.OpeningBracket);
    }

    public static AST CheckType(AST given, ElementTree.Type expected, string name, Token token) => expected.GetType().Name == "Var" || given.GetType() == expected
        ? given
        : throw ExceptionCreator.InvalidType(name, given.GetType(), expected, token);
    public static bool CheckType(AST given, ElementTree.Type expected) => expected.GetType().Name == "var" || given.GetType() == expected || (expected.GetType().Name == "object" && CheckType(given, new("Object")));

    private static bool IsIdentifierUnique(Scope scope, string name)
        => GetVariableFromScope(scope, name, true) is null && GetFunctionFromScope(scope, name, true) is null && GetClassFromScope(scope, name, true) is null;
    private static bool IsDeclared(Scope scope, string name) => GetVariableFromScope(scope, name) is not null;
    private static Variable? GetVariableFromScope(Scope scope, string name, bool singleLayer = false) => scope.Search(x => x is Variable variable && variable.Name == name, singleLayer) as Variable;
    public static Function? GetFunctionFromScope(Scope scope, string name, bool singleLayer = false) => scope.Search(x => x is Function function && function.Name == name, singleLayer) as Function;
    public static ElementTree.Object? GetClassFromScope(Scope scope, string name, bool singleLayer = false)
        => scope.Search(x => x is ElementTree.Object @class && @class.Name == name, singleLayer) as ElementTree.Object;
    public static Variable GetValidVariable(Scope scope, string name, Token token)
    {
        if (!IsDeclared(scope, name)) throw ExceptionCreator.NotAssignedInScope(token); // their is currently no variable with this name

        var variable = GetVariableFromScope(scope, name);
        return variable is null ? throw ExceptionCreator.VariableExpected() : variable;
    }
}
