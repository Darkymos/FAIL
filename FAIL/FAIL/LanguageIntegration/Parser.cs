using FAIL.ElementTree;
using FAIL.ElementTree.BinaryOperators;
using FAIL.ElementTree.DataTypes;
using FAIL.Exceptions;
using System.Diagnostics;
using System.Reflection;

namespace FAIL.LanguageIntegration;
internal class Parser
{
    public Tokenizer Tokenizer { get; }

    private IEnumerator<Token>? TokenEnumerator;
    private Token? CurrentToken = null;
    private Token? LastToken = null;

    private static readonly Dictionary<string, Type> DotOperatorMapper = new()
    {
        { "*", typeof(Multiplication) },
        { "/", typeof(Division) },
    };
    private static readonly Dictionary<string, Type> StrokeOperatorMapper = new()
    {
        { "+", typeof(Addition) },
        { "-", typeof(Substraction) },
    };
    private static readonly Dictionary<string, Type> TestOperatorMapper = new()
    {
        { "==", typeof(Equal) },
        { "!=", typeof(NotEqual) },
        { ">=", typeof(GreaterThanOrEqual) },
        { "<=", typeof(LessThanOrEqual) },
        { ">", typeof(GreaterThan) },
        { "<", typeof(LessThan) },
    };
    private static readonly Dictionary<string, Type> SelfAssignmentOperatorMapper = new()
    {
        { "+=", typeof(Addition) },
        { "-=", typeof(Substraction) },
        { "*=", typeof(Multiplication) },
        { "/=", typeof(Division) },
    };
    private static readonly Dictionary<string, Type> IncrementalOperatorMapper = new()
    {
        { "++", typeof(Addition) },
        { "--", typeof(Substraction) },
    };


    public Parser(string file, string fileName)
    {
        Tokenizer = new(file, fileName);
        AcceptAny();
    }


    public AST? Parse()
    {
        if (IsEOT()) return null;

        var ast = ParseCommandList(TokenType.EndOfStatement);
        return IsEOT() ? ast : throw ExceptionCreator.UnexpectedToken(CurrentToken!.Value);
    }

    protected CommandList ParseCommandList(TokenType endOfStatementSign, TokenType? endOfBlockSign = null, params Scope[] shared)
    {
        var commands = new Scope(new(), shared);

        while (!IsEOT() && (endOfBlockSign is null || !IsTypeOf(endOfBlockSign.Value))) 
            commands.Add(ParseCommand(commands, endOfStatementSign, endOfBlockSign)!);

        if (endOfBlockSign is not null) Accept(endOfBlockSign.Value);

        return new CommandList(commands);
    }
    protected AST ParseCommand(Scope scope, 
                               TokenType? endOfStatementSign = null, 
                               TokenType? endOfBlockSign = null)
    {
        AST result;
        var endOfStatementSignRequired = true;

        switch (CurrentToken!.Value.Type)
        {
            case TokenType.Var:
            case TokenType.Void:
            case TokenType.Object:
            case TokenType.DataType:
                result = ParseType(scope, out endOfStatementSignRequired);
                break;

            case TokenType.If:
            case TokenType.While:
            case TokenType.For:
                endOfStatementSignRequired = false;
                var token3 = CurrentToken!.Value;
                AcceptAny();
                Accept(TokenType.OpeningParenthese);
                result = (GetType().GetMethod($"Parse{token3.Type}", BindingFlags.NonPublic | BindingFlags.Instance)!
                                   .Invoke(this, new object[] { scope, token3 }) as AST)!;
                break;

            case TokenType.Log:
            case TokenType.Input:
                var token = CurrentToken!.Value;
                AcceptAny();
                Accept(TokenType.OpeningParenthese);
                result = (Activator.CreateInstance(Type.GetType($"FAIL.ElementTree.{token.Type}")!, ParseCommand(scope, TokenType.ClosingParenthese), token) as AST)!;
                break;

            case TokenType.Return:
                var token2 = CurrentToken!.Value;
                AcceptAny();
                result = (Activator.CreateInstance(Type.GetType($"FAIL.ElementTree.{token2.Type}")!, ParseCommand(scope, endOfStatementSign), token2) as AST)!;
                break;

            case TokenType.Continue:
            case TokenType.Break:
                result = (Activator.CreateInstance(Type.GetType($"FAIL.ElementTree.{CurrentToken!.Value.Type}")!, CurrentToken) as AST)!;
                AcceptAny();
                break;

            default:
                result = ParseTerm(scope);
                break;
        }

        if (endOfStatementSign is not null
            && endOfStatementSignRequired   
            && !IsEOT() 
            && !(endOfBlockSign is not null && IsTypeOf(endOfBlockSign.Value))) 
            Accept(endOfStatementSign!.Value);

        return result;
    }

    protected static ElementTree.DataTypes.Object ParseObject(Type type, Token token) 
        => Activator.CreateInstance(type, token.Value, token);

    protected AST ParseTerm(Scope scope, Calculations calculations = (Calculations)15, AST? heap = null)
    {
        if (calculations.HasFlag(Calculations.Term)) heap = ParseTerm(scope, heap);
        if (calculations.HasFlag(Calculations.DotCalculations)) heap = ParseDotCalculation(scope, heap);
        if (calculations.HasFlag(Calculations.StrokeCalculations)) heap = ParseStrokeCalculation(scope, heap);
        if (calculations.HasFlag(Calculations.TestOperations)) heap = ParseTestOperations(scope, heap);

        return heap!;
    }
    protected AST? ParseTerm(Scope scope, AST? heap)
    {
        var token = CurrentToken;

        if (IsTypeOf(TokenType.OpeningParenthese))
        {
            AcceptAny();
            AST? subTerm;

            if (IsTypeOf(TokenType.StrokeCalculation) && HasValue("-"))
            {
                AcceptAny();
                subTerm = ParseTerm(scope,
                                    Calculations.DotCalculations | Calculations.StrokeCalculations,
                                    new Substraction(new ElementTree.DataTypes.Object(0),
                                                     ParseTerm(scope, Calculations.Term)));
            }
            else subTerm = ParseTerm(scope);

            Accept(TokenType.ClosingParenthese);
            return subTerm;
        }

        if (IsTypeOf(TokenType.StrokeCalculation) && HasValue("-"))
        {
            AcceptAny();

            return ParseTerm(scope,
                             Calculations.DotCalculations | Calculations.StrokeCalculations,
                             new Substraction(new ElementTree.DataTypes.Object(0),
                                              ParseTerm(scope, Calculations.Term)));
        }

        if (IsTypeOf(TokenType.Number))
        {
            AcceptAny();

            if (token!.Value.Value is int) return ParseObject(typeof(Integer), token!.Value);
            if (token!.Value.Value is double) return ParseObject(typeof(ElementTree.DataTypes.Double), token!.Value);
        }
        if (IsTypeOf(TokenType.String))
        {
            AcceptAny();

            return ParseObject(typeof(ElementTree.DataTypes.String), token!.Value);
        }
        if (IsTypeOf(TokenType.Boolean))
        {
            AcceptAny();

            return ParseObject(typeof(ElementTree.DataTypes.Boolean), token!.Value);
        }

        if (IsTypeOf(TokenType.Identifier))
        {
            AcceptAny();

            if (IsTypeOf(TokenType.Assignment)) return ParseAssignment(scope, token!.Value);
            if (IsTypeOf(TokenType.SelfAssignment)) return ParseSelfAssignment(scope, token!.Value);
            if (IsTypeOf(TokenType.OpeningParenthese)) return ParseFunctionCall(scope, token);
            if (IsTypeOf(TokenType.IncrementalOperator)) return ParseIncrementalOperator(scope, token!.Value);
            return new Reference(scope, token);
        }

        return heap!;
    }
    protected AST? ParseDotCalculation(Scope scope, AST? heap)
    {
        if (IsEOT() || !IsTypeOf(TokenType.DotCalculation)) return heap;

        if (DotOperatorMapper.ContainsKey(GetValue()))
        {
            var token = CurrentToken;
            AcceptAny();
            var secondParameter = ParseTerm(scope, Calculations.Term);
            return ParseTerm(scope,
                             Calculations.DotCalculations | Calculations.StrokeCalculations | Calculations.TestOperations,
                             Activator.CreateInstance(DotOperatorMapper[GetValue(token)], heap, secondParameter, token));
        }

        return heap;
    }
    protected AST? ParseStrokeCalculation(Scope scope, AST? heap)
    {
        if (IsEOT() || !IsTypeOf(TokenType.StrokeCalculation)) return heap;

        if (StrokeOperatorMapper.ContainsKey(GetValue()))
        {
            var token = CurrentToken;
            AcceptAny();
            var secondParameter = ParseTerm(scope, Calculations.DotCalculations | Calculations.Term);
            return ParseTerm(scope,
                             Calculations.StrokeCalculations | Calculations.TestOperations,
                             Activator.CreateInstance(StrokeOperatorMapper[GetValue(token)], heap, secondParameter, token));
        }

        return heap;
    }
    protected AST? ParseTestOperations(Scope scope, AST? heap)
    {
        if (IsEOT() || !IsTypeOf(TokenType.TestOperator)) return heap;

        if (TestOperatorMapper.ContainsKey(GetValue()))
        {
            var token = CurrentToken;
            AcceptAny();
            var secondParameter = ParseTerm(scope, Calculations.StrokeCalculations | Calculations.DotCalculations | Calculations.Term);
            return ParseTerm(scope, 
                             Calculations.TestOperations, 
                             Activator.CreateInstance(TestOperatorMapper[GetValue(token)], heap, secondParameter, token));
        }

        return heap;
    }

    protected AST ParseType(Scope scope, out bool endOfStatementSignRequiredVariable)
    {
        var type = CurrentToken!.Value.Type;
        var identifier = AcceptAny()!.Value;
        Accept(TokenType.Identifier);

        return IsTypeOf(TokenType.OpeningParenthese)
            ? ParseFunction(scope, out endOfStatementSignRequiredVariable, type, identifier)
            : ParseVar(scope, out endOfStatementSignRequiredVariable, type, identifier);
    }
    protected AST ParseVar(Scope scope, out bool endOfStatementSignRequiredVariable, TokenType type, Token identifier)
    {
        endOfStatementSignRequiredVariable = true;

        if (IsAssigned(scope, identifier.Value)) throw ExceptionCreator.AlreadyAssignedInScope(identifier!.Value.Value);

        if (!IsTypeOf(TokenType.Assignment)) return new Variable(identifier.Value, type.ToString(), token: identifier);
        Accept(TokenType.Assignment);

        return new Variable(identifier.Value, 
                            type.ToString(),
                            ParseCommand(scope), 
                            identifier);
    }
    protected AST ParseFunction(Scope scope, out bool endOfStatementSignRequiredVariable, TokenType type, Token identifier)
    {
        endOfStatementSignRequiredVariable = false;

        if (IsAssigned(scope, identifier.Value)) throw ExceptionCreator.AlreadyAssignedInScope(identifier!.Value.Value);

        Accept(TokenType.OpeningParenthese);
        var argList = ParseCommandList(TokenType.Separator, TokenType.ClosingParenthese);

        var body = ParseBody(argList.Commands, scope);
        if (type != TokenType.Void && body.Commands.Entries.Last() is not Return) 
            throw ExceptionCreator.FunctionMustReturnValue(identifier.Value);

        return new Function(identifier.Value, type.ToString(), argList, body, identifier);
    }

    protected AST ParseAssignment(Scope scope, Token token)
    {
        Accept(TokenType.Assignment);
        return new Assignment(GetValidVariable(scope, token.Value, token), ParseCommand(scope));
    }
    protected AST ParseSelfAssignment(Scope scope, Token token)
    {
        var op = CurrentToken;
        Accept(TokenType.SelfAssignment);

        var variable = GetValidVariable(scope, token.Value, token);
        return new Assignment(variable,
                              Activator.CreateInstance(SelfAssignmentOperatorMapper[GetValue(op)],
                                                       variable,
                                                       ParseCommand(scope),
                                                       token));
    }
    protected AST ParseFunctionCall(Scope scope, Token? token)
    {
        Accept(TokenType.OpeningParenthese);
        var parameters = ParseCommandList(TokenType.Separator, TokenType.ClosingParenthese, scope);
        return new FunctionCall(GetFunctionFromScope(scope, token!.Value.Value), parameters);
    }
    protected AST ParseIncrementalOperator(Scope scope, Token token)
    {
        var op = CurrentToken;
        AcceptAny();

        var variable = GetValidVariable(scope, token.Value, token);
        return new Assignment(variable,
                              Activator.CreateInstance(IncrementalOperatorMapper[GetValue(op)],
                                                       variable,
                                                       new ElementTree.DataTypes.Object(1),
                                                       token));
    }

    protected AST ParseIf(Scope scope, Token token)
    {
        var testCommand = ParseCommand(scope, TokenType.ClosingParenthese);

        var ifBody = ParseBody(scope);

        if (!IsEOT() && IsTypeOf(TokenType.Else))
        {
            AcceptAny();

            return new If(testCommand!,
                          ifBody,
                          IsTypeOf(TokenType.If)
                            ? ParseCommand(scope)
                            : ParseBody(scope),
                          token);
        }

        return new If(testCommand!, ifBody, null, token);
    }
    protected AST ParseWhile(Scope scope, Token token)
    {
        var testCommand = ParseCommand(scope, TokenType.ClosingParenthese);

        return new While(testCommand!, ParseBody(scope), token);
    }
    protected AST ParseFor(Scope scope, Token token)
    {
        var internalScope = new Scope(new());
        var iteratorVariable = ParseCommand(internalScope, TokenType.EndOfStatement);
        internalScope.Add(iteratorVariable);
        var iteratorTest = ParseCommand(internalScope, TokenType.EndOfStatement);
        var iteratorAction = ParseCommand(internalScope, TokenType.ClosingParenthese);

        return new For(iteratorVariable, iteratorTest, iteratorAction, ParseBody(internalScope, scope), token);
    }
    
    protected CommandList ParseBody(params Scope[] scopes)
    {
        if (IsTypeOf(TokenType.OpeningBracket))
        {
            Accept(TokenType.OpeningBracket);
            return ParseCommandList(TokenType.EndOfStatement, TokenType.ClosingBracket, scopes);
        }
        else return new(new(new() { ParseCommand(new(new(), scopes)) }));
    }


    private bool IsEOT() => CurrentToken is null;
    private bool IsTypeOf(TokenType type) => CurrentToken!.Value.Type == type;
    private static bool IsTypeOf(TokenType type, Token? token) => token!.Value.Type == type;
    private bool HasValue(dynamic value) => CurrentToken!.Value.Value == value;
    private dynamic GetValue() => CurrentToken!.Value.Value;
    private static bool HasValue(Token? token, dynamic value) => token!.Value.Value.GetType() == value.GetType() && token!.Value.Value == value;
    private static dynamic GetValue(Token? token) => token!.Value.Value;

    private static bool IsAssigned(Scope scope, string name)
    {
        if (GetVariableFromScope(scope, name) is not null) return true;
        if (GetFunctionFromScope(scope, name) is not null) return true;
        return false;
    }
    private static Variable? GetVariableFromScope(Scope scope, string name)
        => scope.Search(x => x is Variable variable && variable.Name == name) as Variable;
    public static Function? GetFunctionFromScope(Scope scope, string name)
        => scope.Search(x => x is Function function && function.Name == name) as Function;
    public static Variable GetValidVariable(Scope scope, string name, Token token)
    {
        if (!IsAssigned(scope, name)) throw ExceptionCreator.NotAssignedInScope(token);

        var variable = GetVariableFromScope(scope, name);
        return variable is null ? throw ExceptionCreator.VariableExpected() : variable;
    }

    private Token? AcceptAny()
    {
        if (TokenEnumerator is null) TokenEnumerator = Tokenizer.GetEnumerator();

        LastToken = CurrentToken;

        try
        {
            TokenEnumerator.MoveNext();
            CurrentToken = TokenEnumerator.Current;
        }
        catch (StopIterationException)
        {
            CurrentToken = null;
            return null;
        }

        return CurrentToken;
    }
    private Token? Accept(TokenType expected)
    {
        if (TokenEnumerator is null) TokenEnumerator = Tokenizer.GetEnumerator();
        if (!expected.HasFlag(CurrentToken!.Value.Type)) throw ExceptionCreator.WrongToken(CurrentToken!.Value, expected);

        LastToken = CurrentToken;

        try
        {
            TokenEnumerator.MoveNext();
            CurrentToken = TokenEnumerator.Current;
        }
        catch (StopIterationException)
        {
            CurrentToken = null;
            return null;
        }

        return CurrentToken;
    }
}
