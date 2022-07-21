using FAIL.ElementTree;
using FAIL.ElementTree.BinaryOperators;
using FAIL.Exceptions;

namespace FAIL.LanguageIntegration;
internal class Parser
{
    public Tokenizer Tokenizer { get; }

    private IEnumerator<Token>? TokenEnumerator;
    private Token? CurrentToken = null;

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
    protected AST? ParseCommand(Scope scope, 
                                TokenType endOfStatementSign, 
                                TokenType? endOfBlockSign = null, 
                                bool acceptEndOfStatementSign = true)
    {
        AST? result;
        var endOfStatementSignRequired = true;

        if (IsTypeOf(TokenType.KeyWord)) result = (KeyWord)CurrentToken!.Value.Value switch
        {
            KeyWord.Log => ParseLog(scope),
            KeyWord.Input => ParseInput(scope),
            KeyWord.Var => ParseVar(scope),
            KeyWord.Void => ParseFunction(scope, out endOfStatementSignRequired),
            KeyWord.Object => ParseFunction(scope, out endOfStatementSignRequired),
            KeyWord.Return => ParseReturn(scope, endOfStatementSign),
            KeyWord.If => ParseIf(scope, out endOfStatementSignRequired),
            KeyWord.While => ParseWhile(scope, out endOfStatementSignRequired),
            KeyWord.Break => ParseBreak(scope, endOfStatementSign),
            KeyWord.Continue => ParseContinue(scope, endOfStatementSign),
            _ => throw new NotImplementedException(),
        };
        else result = ParseTerm(scope);

        if (acceptEndOfStatementSign 
            && endOfStatementSignRequired 
            && !IsEOT() 
            && !(endOfBlockSign is not null && IsTypeOf(endOfBlockSign.Value))) 
            Accept(endOfStatementSign);

        return result;
    }

    protected ElementTree.DataTypes.Object? ParseObject() => new(CurrentToken!.Value.Value, CurrentToken);

    protected AST? ParseTerm(Scope scope, Calculations calculations = (Calculations)15, AST? heap = null)
    {
        if (calculations.HasFlag(Calculations.Term)) heap = ParseTerm(scope, heap);
        if (calculations.HasFlag(Calculations.DotCalculations)) heap = ParseDotCalculation(scope, heap);
        if (calculations.HasFlag(Calculations.StrokeCalculations)) heap = ParseStrokeCalculation(scope, heap);
        if (calculations.HasFlag(Calculations.TestOperations)) heap = ParseTestOperations(scope, heap);

        return heap;
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

        if (IsTypeOf(TokenType.Number) || IsTypeOf(TokenType.String) || IsTypeOf(TokenType.Boolean))
        {
            AcceptAny();

            return new ElementTree.DataTypes.Object(token!.Value.Value, token);
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

        return heap;
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

    protected AST ParseLog(Scope scope)
    {
        var logToken = CurrentToken;

        AcceptAny();
        Accept(TokenType.OpeningParenthese);

        return new Log(ParseCommand(scope, TokenType.ClosingParenthese), logToken);
    }
    protected AST ParseInput(Scope scope)
    {
        var inputToken = CurrentToken;

        AcceptAny();
        Accept(TokenType.OpeningParenthese);

        return new Input(ParseCommand(scope, TokenType.ClosingParenthese)!, inputToken);
    }
    protected AST? ParseVar(Scope scope)
    {
        var identifier = AcceptAny();

        if (IsAssigned(scope, identifier!.Value.Value)) throw ExceptionCreator.AlreadyAssignedInScope(identifier!.Value.Value);

        Accept(TokenType.Identifier);

        if (!IsTypeOf(TokenType.Assignment)) return new Variable(identifier!.Value.Value, token: identifier);

        Accept(TokenType.Assignment);
        return new Variable(identifier!.Value.Value, 
                            ParseCommand(scope, TokenType.EndOfStatement, acceptEndOfStatementSign: false), 
                            identifier);
    }
    protected AST? ParseFunction(Scope scope, out bool endOfStatementSignRequiredVariable)
    {
        endOfStatementSignRequiredVariable = false;

        var returnType = CurrentToken;
        var identifier = AcceptAny();

        if (IsAssigned(scope, identifier!.Value.Value)) throw ExceptionCreator.AlreadyAssignedInScope(identifier!.Value.Value);

        AcceptAny();
        Accept(TokenType.OpeningParenthese);
        var argList = ParseCommandList(TokenType.Separator, TokenType.ClosingParenthese);
        Accept(TokenType.OpeningBracket);

        var cmdList = ParseCommandList(TokenType.EndOfStatement, TokenType.ClosingBracket, scope, argList.Commands);
        if (returnType!.Value.Value != KeyWord.Void && cmdList.Commands.Entries.Last() is not Return) 
            throw ExceptionCreator.FunctionMustReturnValue(identifier!.Value.Value);

        return new Function(identifier!.Value.Value, returnType!.Value.Value.ToString(), argList, cmdList, identifier);
    }
    protected AST ParseAssignment(Scope scope, Token token)
    {
        Accept(TokenType.Assignment);
        return new Assignment(GetValidVariable(scope, token.Value), ParseCommand(scope, TokenType.EndOfStatement, acceptEndOfStatementSign: false));
    }
    protected AST ParseSelfAssignment(Scope scope, Token token)
    {
        var op = CurrentToken;
        Accept(TokenType.SelfAssignment);

        var variable = GetValidVariable(scope, token.Value);
        return new Assignment(variable,
                              Activator.CreateInstance(SelfAssignmentOperatorMapper[GetValue(op)],
                                                       variable,
                                                       ParseCommand(scope, TokenType.EndOfStatement, acceptEndOfStatementSign: false),
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

        var variable = GetValidVariable(scope, token.Value);
        return new Assignment(variable,
                              Activator.CreateInstance(IncrementalOperatorMapper[GetValue(op)],
                                                       variable,
                                                       new ElementTree.DataTypes.Object(1),
                                                       token));
    }
    protected AST? ParseReturn(Scope scope, TokenType endOfStatementSign)
    {
        var returnToken = CurrentToken;
        AcceptAny();
        return new Return(ParseCommand(scope, endOfStatementSign), returnToken);
    }
    protected AST ParseIf(Scope scope, out bool endOfStatementSignRequiredVariable)
    {
        endOfStatementSignRequiredVariable = false;

        var token = CurrentToken;
        AcceptAny();

        Accept(TokenType.OpeningParenthese);
        var testCommand = ParseCommand(scope, TokenType.ClosingParenthese);

        Accept(TokenType.OpeningBracket);
        var ifBody = ParseCommandList(TokenType.EndOfStatement, TokenType.ClosingBracket, scope);

        if (!IsEOT() && IsTypeOf(TokenType.KeyWord) && HasValue(KeyWord.Else))
        {
            AcceptAny();

            if (IsTypeOf(TokenType.KeyWord) && HasValue(KeyWord.If))
            {
                return new If(testCommand!, ifBody, ParseIf(scope, out var dummy), token);
            }

            Accept(TokenType.OpeningBracket);
            return new If(testCommand!, ifBody, ParseCommandList(TokenType.EndOfStatement, TokenType.ClosingBracket, scope), token); 
        }

        return new If(testCommand!, ifBody, null, token);
    }
    protected AST ParseWhile(Scope scope, out bool endOfStatementSignRequiredVariable)
    {
        endOfStatementSignRequiredVariable = false;

        var token = CurrentToken;
        AcceptAny();

        Accept(TokenType.OpeningParenthese);
        var testCommand = ParseCommand(scope, TokenType.ClosingParenthese);

        Accept(TokenType.OpeningBracket);
        var body = ParseCommandList(TokenType.EndOfStatement, TokenType.ClosingBracket, scope);

        return new While(testCommand!, body, token);
    }
    protected AST ParseBreak(Scope scope, TokenType endOfStatementSign)
    {
        var token = CurrentToken;
        AcceptAny();

        return new Break(token);
    }
    protected AST ParseContinue(Scope scope, TokenType endOfStatementSign)
    {
        var token = CurrentToken;
        AcceptAny();

        return new Continue(token);
    }


    private bool IsEOT() => CurrentToken is null;
    private bool IsTypeOf(TokenType type) => CurrentToken!.Value.Type == type;
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
    public static Variable GetValidVariable(Scope scope, string name)
    {
        if (!IsAssigned(scope, name)) throw ExceptionCreator.NotAssignedInScope(name);

        var variable = GetVariableFromScope(scope, name);
        return variable is null ? throw ExceptionCreator.VariableExpected() : variable;
    }

    private Token? AcceptAny()
    {
        if (TokenEnumerator is null) TokenEnumerator = Tokenizer.GetEnumerator();

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
