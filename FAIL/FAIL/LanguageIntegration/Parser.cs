using FAIL.ElementTree;
using FAIL.ElementTree.BinaryOperators;
using FAIL.ElementTree.DataTypes;
using FAIL.Exceptions;
using System.Reflection;

namespace FAIL.LanguageIntegration;
internal class Parser
{
    public Tokenizer Tokenizer { get; } // here we will emit our tokens to work with
    protected IEnumerator<Token>? TokenEnumerator; // used to receive the tokens from the Tokenizer

    protected Token? CurrentToken => TokenStack[^1];
    protected Token? LastToken => TokenStack[^2];
    protected List<Token?> TokenStack = new();

    // they just map tokens to types of the element tree
    // kinda redundant, but i haven't found a way to replace them yet
    protected static readonly Dictionary<string, System.Type> ConversionOperatorMapper = new()
    {
        { "as", typeof(TypeConversion) },
    };
    protected static readonly Dictionary<string, System.Type> DotOperatorMapper = new()
    {
        { "*", typeof(Multiplication) },
        { "/", typeof(Division) },
    };
    protected static readonly Dictionary<string, System.Type> StrokeOperatorMapper = new()
    {
        { "+", typeof(Addition) },
        { "-", typeof(Substraction) },
    };
    protected static readonly Dictionary<string, System.Type> TestOperatorMapper = new()
    {
        { "==", typeof(Equal) },
        { "!=", typeof(NotEqual) },
        { ">=", typeof(GreaterThanOrEqual) },
        { "<=", typeof(LessThanOrEqual) },
        { ">", typeof(GreaterThan) },
        { "<", typeof(LessThan) },
    };
    protected static readonly Dictionary<string, System.Type> SelfAssignmentOperatorMapper = new()
    {
        { "+=", typeof(Addition) },
        { "-=", typeof(Substraction) },
        { "*=", typeof(Multiplication) },
        { "/=", typeof(Division) },
    };
    protected static readonly Dictionary<string, System.Type> IncrementalOperatorMapper = new()
    {
        { "++", typeof(Addition) },
        { "--", typeof(Substraction) },
    };


    public Parser(string code, string fileName)
    {
        Tokenizer = new(code, fileName);
        _ = AcceptAny(); // get the first token
    }


    public CommandList Parse()
    {
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
            _ = AcceptAny();
            _ = Accept(TokenType.OpeningParenthese);
            return (GetType().GetMethod($"Parse{token3.Type}", BindingFlags.NonPublic | BindingFlags.Instance)!
                             .Invoke(this, new object[] { scope, token3 }) as AST)!;
        }
        AST ParseBuiltInFunction(Scope scope)
        {
            var token = CurrentToken!.Value;
            _ = AcceptAny();
            _ = Accept(TokenType.OpeningParenthese);
            return (Activator.CreateInstance(System.Type.GetType($"FAIL.ElementTree.{token.Type}")!,
                                             ParseCommand(scope, TokenType.ClosingParenthese), token) as AST)!;
        }
        AST ParseSimpleStatement(Scope scope)
        {
            var token2 = CurrentToken!.Value;
            _ = AcceptAny();
            return (Activator.CreateInstance(System.Type.GetType($"FAIL.ElementTree.{token2.Type}")!,
                                             ParseCommand(scope, endOfStatementSign), token2) as AST)!;
        }
        AST ParseSimpleKeyword()
        {
            var result = (Activator.CreateInstance(System.Type.GetType($"FAIL.ElementTree.{CurrentToken!.Value.Type}")!, CurrentToken) as AST)!;
            _ = AcceptAny();
            return result;
        }

        var result = CurrentToken!.Value.Type switch
        {
            TokenType.Var or TokenType.Void or TokenType.DataType => ParseType(scope, out isBlock),
            TokenType.If or TokenType.While or TokenType.For => ParseBlockStatement(scope, out isBlock),
            TokenType.Log or TokenType.Input => ParseBuiltInFunction(scope),
            TokenType.Return => ParseSimpleStatement(scope),
            TokenType.Continue or TokenType.Break => ParseSimpleKeyword(),
            _ => ParseTerm(scope)
        };

        if (endOfStatementSign is not null // command must have a endOfStatementSign
            && !isBlock
            && (endOfBlockSign is null || !IsTypeOf(endOfBlockSign.Value))) // is ther an endOfBlockSign (like TokenType.ClosingParenthese, see e.g. 'testCommand' of an if)?
            _ = Accept(endOfStatementSign!.Value);

        return result;
    }

    // the corresponding datatype will be invoked (currently just built-in types)
    protected static ElementTree.DataTypes.Object ParseObject(ElementTree.Type type, Token token)
    {
        var builtInType = System.Type.GetType($"FAIL.ElementTree.DataTypes.{type.Name}");
        if (builtInType is not null) return Activator.CreateInstance(builtInType, token.Value, token);

        throw new NotImplementedException();
    }

    // this all parses arithmetically
    protected AST ParseTerm(Scope scope, Calculations calculations = (Calculations)31, AST? heap = null)
    {
        // just the desired categories will be parsed to allow custom rules like a hirarchy, standard (15) is just every category

        if (calculations.HasFlag(Calculations.Term)) heap = ParseTerm(scope, heap);
        if (calculations.HasFlag(Calculations.DotCalculations)) heap = ParseDotCalculation(scope, heap);
        if (calculations.HasFlag(Calculations.StrokeCalculations)) heap = ParseStrokeCalculation(scope, heap);
        if (calculations.HasFlag(Calculations.TestOperations)) heap = ParseTestOperations(scope, heap);
        if (calculations.HasFlag(Calculations.Conversions)) heap = ParseConversion(heap);

        return heap!;
    }
    protected AST? ParseTerm(Scope scope, AST? heap)
    {
        var token = CurrentToken;

        // add one level on top
        if (IsTypeOf(TokenType.OpeningParenthese))
        {
            _ = AcceptAny();
            AST? subTerm;

            // negative number hack (0 - value -> substraction)
            if (IsTypeOf(TokenType.StrokeCalculation) && HasValue("-"))
            {
                _ = AcceptAny();
                subTerm = ParseTerm(scope,
                                    Calculations.DotCalculations | Calculations.StrokeCalculations,
                                    new Substraction(new Integer(0),
                                                     ParseTerm(scope, Calculations.Term)));
            }
            else subTerm = ParseTerm(scope);

            _ = Accept(TokenType.ClosingParenthese);
            return subTerm;
        }

        // negative number hack (0 - value -> substraction)
        if (IsTypeOf(TokenType.StrokeCalculation) && HasValue("-"))
        {
            _ = AcceptAny();

            return ParseTerm(scope,
                             Calculations.DotCalculations | Calculations.StrokeCalculations,
                             new Substraction(new Integer(0),
                                              ParseTerm(scope, Calculations.Term)));
        }

        // currently a bit redundant code, until the type system is finally implemented
        if (IsTypeOf(TokenType.Number))
        {
            _ = AcceptAny();

            if (token!.Value.Value is int) return ParseObject(new("Integer"), token!.Value);
            if (token!.Value.Value is double) return ParseObject(new("Double"), token!.Value);
        }
        if (IsTypeOf(TokenType.String))
        {
            _ = AcceptAny();

            return ParseObject(new("String"), token!.Value);
        }
        if (IsTypeOf(TokenType.Boolean))
        {
            _ = AcceptAny();

            return ParseObject(new("Boolean"), token!.Value);
        }

        // any non-string text
        if (IsTypeOf(TokenType.Identifier))
        {
            _ = AcceptAny();

            if (IsTypeOf(TokenType.Assignment)) return ParseAssignment(scope, token!.Value); // test = 42;
            if (IsTypeOf(TokenType.SelfAssignment)) return ParseSelfAssignment(scope, token!.Value); // test += 42;
            if (IsTypeOf(TokenType.OpeningParenthese)) return ParseFunctionCall(scope, token); // Test();
            if (IsTypeOf(TokenType.IncrementalOperator)) return ParseIncrementalOperator(scope, token!.Value); // test++;

            return new Reference(Parser.GetValidVariable(scope, token!.Value.Value, token!.Value), token);
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
            _ = AcceptAny();
            var secondParameter = ParseTerm(scope, Calculations.Term);
            return ParseTerm(scope,
                             Calculations.DotCalculations | Calculations.StrokeCalculations | Calculations.Conversions | Calculations.TestOperations,
                             Activator.CreateInstance(DotOperatorMapper[GetValue(token)], heap, secondParameter, token));
        }

        return heap; // no dot calculation (possibly an error)
    }
    protected AST? ParseStrokeCalculation(Scope scope, AST? heap)
    {
        if (IsEOT() || !IsTypeOf(TokenType.StrokeCalculation)) return heap; // there is not stroke calculation

        // get the element tree type and invoke and return it -> StrokeOperatorMapper
        if (StrokeOperatorMapper.ContainsKey(GetValue()))
        {
            var token = CurrentToken;
            _ = AcceptAny();
            var secondParameter = ParseTerm(scope, Calculations.DotCalculations | Calculations.Term);
            return ParseTerm(scope,
                             Calculations.StrokeCalculations | Calculations.Conversions | Calculations.TestOperations,
                             Activator.CreateInstance(StrokeOperatorMapper[GetValue(token)], heap, secondParameter, token));
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
            _ = AcceptAny();
            var secondParameter = ParseTerm(scope, Calculations.StrokeCalculations | Calculations.DotCalculations | Calculations.Term);
            return ParseTerm(scope,
                             Calculations.TestOperations | Calculations.Conversions,
                             Activator.CreateInstance(TestOperatorMapper[GetValue(token)], heap, secondParameter, token));
        }

        return heap; // no test operator (possibly an error)
    }
    protected AST? ParseConversion(AST? heap)
    {
        if (IsEOT() || !IsTypeOf(TokenType.Conversion)) return heap; // there is no conversion

        if (ConversionOperatorMapper.ContainsKey(GetValue()))
        {
            var token = CurrentToken;
            _ = AcceptAny();
            var newType = new ElementTree.Type(GetValue());
            _ = Accept(TokenType.DataType);
            return Activator.CreateInstance(ConversionOperatorMapper[GetValue(token)], heap, newType, token);
        }

        return heap; // no conversion (possibly an error)
    }

    // everthing related to types (e.g. declarations)
    protected AST ParseType(Scope scope, out bool isBlock)
    {
        var type = CurrentToken!.Value;
        var identifier = AcceptAny()!.Value;
        _ = Accept(TokenType.Identifier);

        return IsTypeOf(TokenType.OpeningParenthese)
            ? ParseFunction(scope, out isBlock, type, identifier)
            : ParseVar(scope, out isBlock, type, identifier);
    }
    protected AST ParseVar(Scope scope, out bool isBlock, Token type, Token identifier)
    {
        isBlock = false;

        // identifier must be unique in scope (variables AND functions), local are superior to shared ones (identifier doesn't need to be unique)
        if (IsAssigned(scope, identifier.Value)) throw ExceptionCreator.AlreadyAssignedInScope(identifier.Value);

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
                              Activator.CreateInstance(SelfAssignmentOperatorMapper[GetValue(op)],
                                                       variable,
                                                       newValue,
                                                       token));
    } // test += 42;
    protected AST ParseFunctionCall(Scope scope, Token? token)
    {
        _ = Accept(TokenType.OpeningParenthese);
        var parameters = ParseCommandList(TokenType.Separator, TokenType.ClosingParenthese, scope);
        return new FunctionCall(GetFunctionFromScope(scope, token!.Value.Value), parameters, token);
    } // Test();
    protected AST ParseIncrementalOperator(Scope scope, Token token)
    {
        var op = CurrentToken;
        _ = AcceptAny();

        var variable = GetValidVariable(scope, token.Value, token);
        return new Assignment(variable,
                              Activator.CreateInstance(IncrementalOperatorMapper[GetValue(op)],
                                                       variable,
                                                       new Integer(1),
                                                       token));
    } // test++;

    // block statements
    protected AST ParseIf(Scope scope, Token token)
    {
        var testCommand = ParseCommand(scope, TokenType.ClosingParenthese);

        var ifBody = ParseBody(scope);

        if (!IsEOT() && IsTypeOf(TokenType.Else))
        {
            _ = AcceptAny();

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
    protected CommandList ParseBody(params Scope[] scopes)
    {
        if (IsTypeOf(TokenType.OpeningBracket))
        {
            _ = Accept(TokenType.OpeningBracket);
            return ParseCommandList(TokenType.EndOfStatement, TokenType.ClosingBracket, scopes);
        }
        else return new(new Scope(new List<AST>() { ParseCommand(new Scope(scopes)) }));
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

    protected static bool IsAssigned(Scope scope, string name)
    {
        if (GetVariableFromScope(scope, name) is not null) return true; // variable with the name found in scope
        return false; // unassigned yet
    }
    protected static Variable? GetVariableFromScope(Scope scope, string name)
        => scope.Search(x => x is Variable variable && variable.Name == name) as Variable;
    public static Function? GetFunctionFromScope(Scope scope, string name)
        => scope.Search(x => x is Function function && function.Name == name) as Function;
    public static Variable GetValidVariable(Scope scope, string name, Token token)
    {
        if (!IsAssigned(scope, name)) throw ExceptionCreator.NotAssignedInScope(token); // their is currently no variable with this name

        var variable = GetVariableFromScope(scope, name);
        return variable is null ? throw ExceptionCreator.VariableExpected() : variable;
    }

    protected Token? AcceptAny()
    {
        TokenEnumerator ??= Tokenizer.GetEnumerator(); // initialize the TokenEnumerator

        // LastToken could be useful, but there is also the TokenStack

        try
        {
            // get next token
            _ = TokenEnumerator.MoveNext();
            TokenStack.Add(TokenEnumerator.Current);
        }
        catch (StopIterationException) // end of file reached
        {
            TokenStack.Add(null);
            return null;
        }

        return CurrentToken;
    }
    protected Token? Accept(TokenType expected)
    {
        TokenEnumerator ??= Tokenizer.GetEnumerator(); // initialize the TokenEnumerator

        if (!expected.HasFlag(CurrentToken!.Value.Type)) throw ExceptionCreator.WrongToken(CurrentToken!.Value, expected);

        try
        {
            // get next token
            _ = TokenEnumerator.MoveNext();
            TokenStack.Add(TokenEnumerator.Current);
        }
        catch (StopIterationException) // end of file reached
        {
            TokenStack.Add(null);
            return null;
        }

        return CurrentToken;
    }
}
