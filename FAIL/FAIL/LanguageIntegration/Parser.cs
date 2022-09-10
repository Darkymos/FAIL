using FAIL.BuiltIn.DataTypes;
using FAIL.ElementTree;
using FAIL.Metadata;
using System.Reflection;
using static FAIL.BuiltIn.BuiltInFunctions;

namespace FAIL.LanguageIntegration;
internal class Parser : IParser
{
    protected List<Token>? Tokens;
    protected int CurrentIndex = 0;
    protected Token? CurrentToken => CurrentIndex < Tokens!.Count ? Tokens![CurrentIndex] : null;
    protected Token? LastToken => CurrentIndex != 0 ? Tokens![CurrentIndex - 1] : null;

    // they just map tokens to types of the element tree
    // kinda redundant, but i haven't found a way to replace them yet
    protected static readonly Dictionary<string, BinaryOperation> DotOperatorMapper = new()
    {
        { "*", BinaryOperation.Multiplication },
        { "/", BinaryOperation.Division },
    };
    protected static readonly Dictionary<string, BinaryOperation> StrokeOperatorMapper = new()
    {
        { "+", BinaryOperation.Addition },
        { "-", BinaryOperation.Substraction },
    };
    protected static readonly Dictionary<string, BinaryOperation> TestOperatorMapper = new()
    {
        { "==", BinaryOperation.Equal },
        { "!=", BinaryOperation.NotEqual },
        { ">=", BinaryOperation.GreaterThanOrEqual },
        { "<=", BinaryOperation.LessThanOrEqual },
        { ">", BinaryOperation.GreaterThan },
        { "<", BinaryOperation.LessThan },
    };
    protected static readonly Dictionary<string, BinaryOperation> LogicalOperatorMapper = new()
    {
        { "or", BinaryOperation.Or },
        { "and", BinaryOperation.And },
        { "||", BinaryOperation.Or },
        { "&&", BinaryOperation.And },
    };
    protected static readonly Dictionary<string, BinaryOperation> SelfAssignmentOperatorMapper = new()
    {
        { "+=", BinaryOperation.Addition },
        { "-=", BinaryOperation.Substraction },
        { "*=", BinaryOperation.Multiplication },
        { "/=", BinaryOperation.Division },
    };
    protected static readonly Dictionary<string, BinaryOperation> IncrementalOperatorMapper = new()
    {
        { "++", BinaryOperation.Addition },
        { "--", BinaryOperation.Substraction },
    };


    public CommandList Call(List<Token> tokens)
    {
        Tokens = tokens;

        if (IsEOT()) return new(); // empty file

        var topLevelStatements = ParseCommandList(TokenType.EndOfStatement);

        // is there a character, that shouldn't be there (a non finished command)?
        return IsEOT() ? topLevelStatements : throw ExceptionCreator.UnexpectedToken(CurrentToken!.Value);
    }

    protected CommandList ParseCommandList(TokenType endOfStatementSign, TokenType? endOfBlockSign = null, params Scope[] shared)
    {
        var commands = new Scope(new(), shared); // owned scope

        bool IsEnd()
        {
            if (IsEOT() && endOfBlockSign is not null)
                throw ExceptionCreator.UnexpectedToken(CurrentToken!.Value);

            var endOfBlock = endOfBlockSign is not null && IsTypeOf(endOfBlockSign.Value);
            var topLevel = IsEOT();

            return endOfBlock || topLevel;
        }
        while (!IsEnd()) commands.Add(ParseCommand(commands, endOfStatementSign, endOfBlockSign)!);

        // most often TokenType.ClosingBracket
        if (endOfBlockSign is not null) _ = Accept(endOfBlockSign.Value);

        return new(commands);
    }
    protected AST ParseCommand(Scope scope, TokenType? endOfStatementSign = null, TokenType? endOfBlockSign = null)
    {
        var isBlock = false; // kinda redundant, but still there, until a better solution is found

        AST ParseBlockStatement(Scope scope, out bool isBlock)
        {
            isBlock = true;
            var token3 = CurrentToken!.Value;
            _ = Accept();
            _ = Accept(TokenType.OpeningParenthese);
            return (GetType().GetMethod($"Parse{token3.Type}", BindingFlags.NonPublic | BindingFlags.Instance)!
                             .Invoke(this, new object[] { scope, token3 }) as AST)!;
        }
        AST ParseSimpleStatement(Scope scope)
        {
            var token2 = CurrentToken!.Value;
            _ = Accept();
            return (Activator.CreateInstance(System.Type.GetType($"FAIL.ElementTree.{token2.Type}")!,
                                             ParseCommand(scope, endOfStatementSign), token2) as AST)!;
        }
        AST ParseSimpleKeyword()
        {
            var result = (Activator.CreateInstance(System.Type.GetType($"FAIL.ElementTree.{CurrentToken!.Value.Type}")!, CurrentToken) as AST)!;
            _ = Accept();
            return result;
        }

        var result = CurrentToken!.Value.Type switch
        {
            TokenType.Var or TokenType.Void or TokenType.DataType => ParseType(scope, out isBlock),
            TokenType.Class => ParseClass(scope, out isBlock),
            TokenType.New => ParseTypeInitialization(scope),
            TokenType.If or TokenType.While or TokenType.For => ParseBlockStatement(scope, out isBlock),
            TokenType.Return => ParseSimpleStatement(scope),
            TokenType.Continue or TokenType.Break => ParseSimpleKeyword(),
            _ => ParseArithmentic(scope)
        };

        if (endOfStatementSign is not null // command must have a endOfStatementSign
            && !isBlock
            && (endOfBlockSign is null || !IsTypeOf(endOfBlockSign.Value))) // is ther an endOfBlockSign (like TokenType.ClosingParenthese, see e.g. 'testCommand' of an if)?
            _ = Accept(endOfStatementSign!.Value);

        return result;
    }

    protected static Instance ParseBuiltInClass(ElementTree.Type type, Token token)
    {
        var builtInType = System.Type.GetType($"FAIL.BuiltIn.DataTypes.{type.Name}");
        if (builtInType is not null) return new Instance(type, token.Value, token);

        throw new NotImplementedException();
    }

    // all of this parses arithmetically
    protected AST ParseArithmentic(Scope scope, AST? heap = null) => ParseArithmentic(scope, CalculationsExtensions.All, heap);
    protected AST ParseArithmentic(Scope scope, Calculations calculations, AST? heap = null)
    {
        // just the desired categories will be parsed to allow custom rules like a hirarchy, standard (15) is just every category

        if (calculations.HasFlag(Calculations.Term)) heap = ParseTerm(scope, heap);
        if (calculations.HasFlag(Calculations.DotCalculations)) heap = ParseDotCalculation(scope, heap);
        if (calculations.HasFlag(Calculations.StrokeCalculations)) heap = ParseStrokeCalculation(scope, heap);
        if (calculations.HasFlag(Calculations.TestOperations)) heap = ParseTestOperations(scope, heap);
        if (calculations.HasFlag(Calculations.LogicalOperations)) heap = ParseLogicalOperations(scope, heap);
        if (calculations.HasFlag(Calculations.Conversions)) heap = ParseConversion(heap);

        return heap!;
    }
    protected AST? ParseTerm(Scope scope, AST? heap)
    {
        var token = CurrentToken;

        // add one level on top
        if (IsTypeOf(TokenType.OpeningParenthese))
        {
            _ = Accept(TokenType.OpeningParenthese);
            var subTerm = ParseArithmentic(scope);
            _ = Accept(TokenType.ClosingParenthese);
            return subTerm;
        }

        // unary stroke operation hack (0 - value -> Negation(value))
        if (IsTypeOf(TokenType.StrokeCalculation))
        {
            var operatorToken = CurrentToken;
            _ = Accept(TokenType.StrokeCalculation);
            return HasValue(operatorToken, "+")
                ? ParseArithmentic(scope, ParseArithmentic(scope, Calculations.Term))
                : ParseArithmentic(scope, new UnaryOperator(UnaryOperation.Negation, ParseArithmentic(scope, Calculations.Term)));
        }

        // not hack (!value -> Not(value) / not value -> Not(value))
        if (IsTypeOf(TokenType.LogicalOperator) && (HasValue("not") || HasValue("!")))
        {
            _ = Accept(TokenType.LogicalOperator);
            return ParseArithmentic(scope, new UnaryOperator(UnaryOperation.Not, ParseArithmentic(scope, Calculations.Term)));
        }

        // currently a bit redundant code, until the type system is finally implemented
        if (IsTypeOf(TokenType.Number))
        {
            _ = Accept();

            if (token!.Value.Value is int) return ParseBuiltInClass(new("Integer"), token!.Value);
            if (token!.Value.Value is double) return ParseBuiltInClass(new("Double"), token!.Value);
        }
        if (IsTypeOf(TokenType.String))
        {
            _ = Accept();

            return ParseBuiltInClass(new("String"), token!.Value);
        }
        if (IsTypeOf(TokenType.Char))
        {
            _ = Accept();

            return ParseBuiltInClass(new("Char"), token!.Value);
        }
        if (IsTypeOf(TokenType.Boolean))
        {
            _ = Accept();

            return ParseBuiltInClass(new("Boolean"), token!.Value);
        }

        // any non-string text
        if (IsTypeOf(TokenType.Identifier))
        {
            _ = Accept();

            if (IsTypeOf(TokenType.Assignment)) return ParseAssignment(scope, token!.Value); // test = 42;
            if (IsTypeOf(TokenType.SelfAssignment)) return ParseSelfAssignment(scope, token!.Value); // test += 42;
            if (IsTypeOf(TokenType.OpeningParenthese)) return ParseFunctionCall(scope, token); // Test();
            if (IsTypeOf(TokenType.IncrementalOperator)) return ParseIncrementalOperator(scope, token!.Value); // test++;
            if (IsTypeOf(TokenType.Accessor)) return ParseTypeMember(scope, token!.Value); // test.ToString();

            return new Reference(Parser.GetValidVariable(scope, token!.Value.Value, token!.Value), scope, token);
        }

        return heap!;
    }
    protected AST? ParseDotCalculation(Scope scope, AST? heap)
    {
        if (IsEOT() || !IsTypeOf(TokenType.DotCalculation)) return heap; // there is no dot calculation

        // get the element tree type and invoke and return it -> DotOperatorMapper
        if (DotOperatorMapper.ContainsKey(GetValue()))
        {
            var token = CurrentToken;
            _ = Accept();
            var secondParameter = ParseArithmentic(scope, Calculations.DotCalculations.GetAbove());
            return ParseArithmentic(scope,
                                    Calculations.DotCalculations.GetSelfAndBelow(),
                                    new BinaryOperator(DotOperatorMapper[GetValue(token)], heap, secondParameter, token));
        }

        return heap; // no dot calculation (possibly an error)
    }
    protected AST? ParseStrokeCalculation(Scope scope, AST? heap)
    {
        if (IsEOT() || !IsTypeOf(TokenType.StrokeCalculation)) return heap; // there is not stroke calculation

        // get the element tree type and invoke and return it -> StrokeOperatorMapper
        var temp = GetValue();
        if (StrokeOperatorMapper.ContainsKey(GetValue()))
        {
            var token = CurrentToken;
            _ = Accept();
            var secondParameter = ParseArithmentic(scope, Calculations.StrokeCalculations.GetAbove());
            return ParseArithmentic(scope,
                             Calculations.StrokeCalculations.GetSelfAndBelow(),
                             new BinaryOperator(StrokeOperatorMapper[GetValue(token)], heap, secondParameter, token));
        }

        return heap; // no stroke calculation (possibly an error)
    }
    protected AST? ParseTestOperations(Scope scope, AST? heap)
    {
        if (IsEOT() || !IsTypeOf(TokenType.TestOperator)) return heap; // there is no test operator

        // get the element tree type and invoke and return it -> testOperatorMapper
        if (TestOperatorMapper.ContainsKey(GetValue()))
        {
            var token = CurrentToken;
            _ = Accept();
            var secondParameter = ParseArithmentic(scope, Calculations.TestOperations.GetAbove());
            return ParseArithmentic(scope,
                                    Calculations.TestOperations.GetSelfAndBelow(),
                                    new BinaryOperator(TestOperatorMapper[GetValue(token)], heap, secondParameter, token));
        }

        return heap; // no test operator (possibly an error)
    }
    protected AST? ParseLogicalOperations(Scope scope, AST? heap)
    {
        if (IsEOT() || !IsTypeOf(TokenType.LogicalOperator)) return heap; // there is no logical operator

        if (LogicalOperatorMapper.ContainsKey(GetValue()))
        {
            var token = CurrentToken;
            _ = Accept();
            var secondParameter = ParseArithmentic(scope, Calculations.LogicalOperations.GetAbove());
            return ParseArithmentic(scope,
                                    Calculations.LogicalOperations.GetSelfAndBelow(),
                                    new BinaryOperator(LogicalOperatorMapper[GetValue(token)], heap, secondParameter, token));
        }

        return heap; // no test operator (possibly an error)
    }
    protected AST? ParseConversion(AST? heap)
    {
        if (IsEOT() || !IsTypeOf(TokenType.Conversion)) return heap; // there is no conversion

        var token = CurrentToken;
        _ = Accept();
        var newType = new ElementTree.Type(GetValue());
        _ = Accept(TokenType.DataType);
        return new TypeConversion(heap!, newType, token);
    }

    // everthing related to types (e.g. declarations)
    protected AST ParseType(Scope scope, out bool isBlock)
    {
        var type = CurrentToken!.Value;
        var identifier = Accept()!.Value;
        _ = Accept(TokenType.Identifier);

        return IsTypeOf(TokenType.OpeningParenthese)
            ? ParseFunction(scope, out isBlock, type, identifier)
            : ParseVar(scope, out isBlock, type, identifier);
    }
    protected AST ParseVar(Scope scope, out bool isBlock, Token type, Token identifier)
    {
        isBlock = false;

        // identifier must be unique in scope (variables AND functions), local are superior to shared ones (identifier doesn't need to be unique)
        if (!IsIdentifierUnique(scope, identifier.Value)) throw ExceptionCreator.AlreadyDeclaredInScope(identifier);

        // unassigned variable (used in function parameters)
        if (!IsTypeOf(TokenType.Assignment)) return new Variable(identifier.Value, new ElementTree.Type(type.Value), null, token: identifier);

        // already assigned variable (get the value)
        _ = Accept(TokenType.Assignment);
        return new Variable(identifier.Value,
                            CheckType(ParseCommand(scope), new ElementTree.Type(type.Value), identifier.Value, identifier),
                            identifier);
    }
    protected AST ParseFunction(Scope scope, out bool isBlock, Token type, Token identifier)
    {
        isBlock = true;

        // functions must declare specific types for their return value to avoid major issues with result types on calculations
        if (type.Type.ToString() == "var") throw ExceptionCreator.SpecificTypeNeeded(identifier.Value, identifier);

        // 'parameters' may be empty
        _ = Accept(TokenType.OpeningParenthese);
        var parameters = ParseCommandList(TokenType.Separator, TokenType.ClosingParenthese);

        // functions must declare specific types for their parameters to avoid major issues with result types on calculations
        foreach (var parameter in parameters.Commands.Entries.Cast<Variable>().Where(parameter => parameter.Type.Name == "var"))
            throw ExceptionCreator.SpecificTypeNeeded(identifier.Value, parameter.Token!.Value);

        var body = ParseBody(parameters.Commands, scope);

        // if there is a return type declared in front of the identifier, there has to be a return a the end (currently)
        if (type.Type != TokenType.Void && body.Commands.Entries.Last() is not Return)
            throw ExceptionCreator.FunctionMustReturnValue(identifier.Value);

        // if there is an return type, we have to check it, funtions without return types may return something, which just won't be validated
        if (type.Type != TokenType.Void)
            _ = CheckType(body.Commands.Entries.Last().GetType(), new ElementTree.Type(type.Value), "return", body.Commands.Entries.Last().Token!.Value);

        var existingFunction = GetFunctionFromScope(scope, identifier.Value) as Function;
        if (existingFunction is not null)
        {
            existingFunction.AddOverload(new(new ElementTree.Type(type.Value), parameters, body));
            return existingFunction;
        }

        // create the function boilerplate WITHOUT any overload, then add it
        var function = new Function(identifier.Value, identifier);
        function.AddOverload(new(new ElementTree.Type(type.Value), parameters, body));

        return function;
    }

    // OOP-related stuff
    protected AST ParseClass(Scope scope, out bool isBlock)
    {
        isBlock = true;

        var identifier = Accept(TokenType.Class);
        if (!IsIdentifierUnique(scope, identifier!.Value.Value)) throw ExceptionCreator.AlreadyDeclaredInScope(identifier!.Value);
        _ = Accept(TokenType.Identifier);

        var members = ParseBody(false, scope);

        return new CustomClass(identifier!.Value.Value, members, identifier);
    }
    protected AST ParseTypeInitialization(Scope scope)
    {
        var typeName = Accept(TokenType.New);
        _ = Accept(TokenType.Identifier);

        _ = Accept(TokenType.OpeningParenthese);
        var parameters = ParseCommandList(TokenType.Separator, TokenType.ClosingParenthese, scope);

        return new Instance(new ElementTree.Type(typeName!.Value.Value), scope, parameters);
    }

    // variable manipulations and function calls
    protected AST ParseAssignment(Scope scope, Token token)
    {
        _ = Accept(TokenType.Assignment);

        var variable = (Variable)GetValidVariable(scope, token.Value, token);
        var newValue = ParseCommand(scope);

        _ = CheckType(newValue.GetType(), variable.GetType(), variable.Name, token);

        return new Assignment(variable, newValue, token);
    } // test = 42;
    protected AST ParseSelfAssignment(Scope scope, Token token)
    {
        var op = CurrentToken;
        _ = Accept(TokenType.SelfAssignment);

        var variable = (Variable)GetValidVariable(scope, token.Value, token);
        var newValue = ParseCommand(scope);

        _ = CheckType(newValue.GetType(), variable.GetType(), variable.Name, token);

        return new Assignment(variable,
                              new BinaryOperator(SelfAssignmentOperatorMapper[GetValue(op)],
                                                 variable,
                                                 newValue,
                                                 token));
    } // test += 42;
    protected AST ParseFunctionCall(Scope scope, Token? token)
    {
        _ = Accept(TokenType.OpeningParenthese);
        var parameters = ParseCommandList(TokenType.Separator, TokenType.ClosingParenthese, scope);
        return Functions.ContainsKey(token!.Value.Value)
            ? new BuiltInFunctionCall(token!.Value.Value, parameters)
            : new FunctionCall(GetFunctionFromScope(scope, token!.Value.Value), parameters, token);
    } // Test();
    protected AST ParseIncrementalOperator(Scope scope, Token token)
    {
        var op = CurrentToken;
        _ = Accept();

        var variable = GetValidVariable(scope, token.Value, token);
        return new Assignment(variable,
                              new BinaryOperator(IncrementalOperatorMapper[GetValue(op)],
                                                 variable,
                                                 new Instance(Integer.Type, 1),
                                                 token));
    } // test++;
    protected AST ParseTypeMember(Scope scope, Token token)
    {
        _ = Accept(TokenType.Accessor);

        var reference = new Reference(GetValidVariable(scope, token.Value, token), scope);

        return new InstanceCall(reference, ParseArithmentic(reference.Variable.Call()!.GetValueAs<CustomClass>().Members.Commands, Calculations.Term));
    } // test.ToString();

    // block statements
    protected AST ParseIf(Scope scope, Token token)
    {
        var testCommand = ParseCommand(scope, TokenType.ClosingParenthese);

        var ifBody = ParseBody(scope);

        if (!IsEOT() && IsTypeOf(TokenType.Else))
        {
            _ = Accept();

            return new If(testCommand!,
                          ifBody,
                          IsTypeOf(TokenType.If)
                            ? ParseCommand(scope) // else if
                            : ParseBody(scope), // else
                          token);
        }

        // no else
        return new If(testCommand!, ifBody, null, token);
    }
    protected AST ParseWhile(Scope scope, Token token)
    {
        var testCommand = ParseCommand(scope, TokenType.ClosingParenthese);

        return new While(testCommand!, ParseBody(scope), token);
    }
    protected AST ParseFor(Scope scope, Token token)
    {
        var internalScope = new Scope(); // special scope for the iterator variable
        var iteratorVariable = ParseCommand(internalScope, TokenType.EndOfStatement); // var i = 0;
        internalScope.Add(iteratorVariable);
        var iteratorTest = ParseCommand(internalScope, TokenType.EndOfStatement); // i < length;
        var iteratorAction = ParseCommand(internalScope, TokenType.ClosingParenthese); // i++

        return new For(iteratorVariable, iteratorTest, iteratorAction, ParseBody(internalScope, scope), token);
    }

    // a body of a statement (see above), surrounded by brackets
    protected CommandList ParseBody(params Scope[] scopes) => ParseBody(true, scopes);
    protected CommandList ParseBody(bool allowSingleStatement, params Scope[] scopes)
    {
        if (IsTypeOf(TokenType.OpeningBracket))
        {
            _ = Accept(TokenType.OpeningBracket);
            return ParseCommandList(TokenType.EndOfStatement, TokenType.ClosingBracket, scopes);
        }
        
        if (allowSingleStatement) return new(new Scope(new List<AST>() { ParseCommand(new Scope(scopes)) }));

        throw ExceptionCreator.InvalidToken(CurrentToken!.Value, TokenType.OpeningBracket);
    }

    // type-system-related stuff
    public static AST CheckType(AST given, ElementTree.Type expected, string name, Token token)
        => expected.GetType().Name == "Var" || given.GetType() == expected
            ? given
            : throw ExceptionCreator.InvalidType(name, given.GetType(), expected, token);
    public static bool CheckType(AST given, ElementTree.Type expected)
    {
        if (expected.GetType().Name == "var" || given.GetType() == expected) return true;
        if (expected.GetType().Name == "object") return CheckType(given, new("Object"));
        return false;
    }


    // simple wrapper methods to keep the code clean (damn nullables :D (but their useful))
    protected bool IsEOT() => CurrentToken is null;
    protected bool IsTypeOf(TokenType type) => CurrentToken!.Value.Type == type;
    protected static bool IsTypeOf(TokenType type, Token? token) => token!.Value.Type == type;
    protected bool HasValue(dynamic value) => CurrentToken!.Value.Value == value;
    protected static bool HasValue(Token? token, dynamic value) => token!.Value.Value.GetType() == value.GetType() && token!.Value.Value == value;
    protected dynamic GetValue() => CurrentToken!.Value.Value;
    protected static dynamic GetValue(Token? token) => token!.Value.Value;

    protected static bool IsIdentifierUnique(Scope scope, string name)
    {
        if (GetVariableFromScope(scope, name, true) is not null) return false;
        if (GetFunctionFromScope(scope, name, true) is not null) return false;
        if (GetClassFromScope(scope, name, true) is not null) return false;

        return true;
    }
    protected static bool IsDeclared(Scope scope, string name)
    {
        if (GetVariableFromScope(scope, name) is not null) return true; // variable with the name found in scope
        return false; // not declared yet
    }
    protected static Variable? GetVariableFromScope(Scope scope, string name, bool singleLayer = false)
        => scope.Search(x => x is Variable variable && variable.Name == name, singleLayer) as Variable;
    public static Function? GetFunctionFromScope(Scope scope, string name, bool singleLayer = false)
        => scope.Search(x => x is Function function && function.Name == name, singleLayer) as Function;
    public static ElementTree.Object? GetClassFromScope(Scope scope, string name, bool singleLayer = false)
        => scope.Search(x => x is ElementTree.Object @class && @class.Name == name, singleLayer) as ElementTree.Object;
    public static Variable GetValidVariable(Scope scope, string name, Token token)
    {
        if (!IsDeclared(scope, name)) throw ExceptionCreator.NotAssignedInScope(token); // their is currently no variable with this name

        var variable = GetVariableFromScope(scope, name);
        return variable is null ? throw ExceptionCreator.VariableExpected() : variable;
    }

    protected Token? Accept(TokenType? expected = null)
    {
        if (expected is not null && !expected!.Value.HasFlag(CurrentToken!.Value.Type)) 
            throw ExceptionCreator.InvalidToken(CurrentToken!.Value, expected!.Value);

        CurrentIndex++;
        return CurrentToken;
    }
}
