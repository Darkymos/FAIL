﻿using FAIL.ElementTree;
using FAIL.ElementTree.BinaryOperators;
using FAIL.ElementTree.DataTypes;
using FAIL.Exceptions;
using System.Diagnostics;
using System.Reflection;

namespace FAIL.LanguageIntegration;
internal class Parser
{
    public Tokenizer Tokenizer { get; } // here we will emit our tokens to work with

    // token storage
    protected IEnumerator<Token>? TokenEnumerator;
    protected Token? CurrentToken = null;
    protected Token? LastToken = null;
    protected List<Token?> TokenStack = new();

    // they just map tokens to types of the element tree
    // kinda redundant, but i haven't found a way to replace them yet
    protected static readonly Dictionary<string, Type> DotOperatorMapper = new()
    {
        { "*", typeof(Multiplication) },
        { "/", typeof(Division) },
    };
    protected static readonly Dictionary<string, Type> StrokeOperatorMapper = new()
    {
        { "+", typeof(Addition) },
        { "-", typeof(Substraction) },
    };
    protected static readonly Dictionary<string, Type> TestOperatorMapper = new()
    {
        { "==", typeof(Equal) },
        { "!=", typeof(NotEqual) },
        { ">=", typeof(GreaterThanOrEqual) },
        { "<=", typeof(LessThanOrEqual) },
        { ">", typeof(GreaterThan) },
        { "<", typeof(LessThan) },
    };
    protected static readonly Dictionary<string, Type> SelfAssignmentOperatorMapper = new()
    {
        { "+=", typeof(Addition) },
        { "-=", typeof(Substraction) },
        { "*=", typeof(Multiplication) },
        { "/=", typeof(Division) },
    };
    protected static readonly Dictionary<string, Type> IncrementalOperatorMapper = new()
    {
        { "++", typeof(Addition) },
        { "--", typeof(Substraction) },
    };


    public Parser(string file, string fileName)
    {
        Tokenizer = new(file, fileName);
        AcceptAny(); // get the first token
    }


    public AST? Parse()
    {
        if (IsEOT()) return null; // empty file

        var ast = ParseCommandList(TokenType.EndOfStatement); // top level statements
        return IsEOT() ? ast : throw ExceptionCreator.UnexpectedToken(CurrentToken!.Value); // is there a character, that shouldn't be there?
    }

    protected CommandList ParseCommandList(TokenType endOfStatementSign, TokenType? endOfBlockSign = null, params Scope[] shared)
    {
        var commands = new Scope(new(), shared); // block/top level scope

        // the end of a block is either the end of the file (top level) or a specific character
        while (!IsEOT() && (endOfBlockSign is null || !IsTypeOf(endOfBlockSign.Value))) 
            commands.Add(ParseCommand(commands, endOfStatementSign, endOfBlockSign)!);

        // most often TokenType.ClosingBracket
        if (endOfBlockSign is not null) Accept(endOfBlockSign.Value);

        return new CommandList(commands);
    }
    protected AST ParseCommand(Scope scope, 
                               TokenType? endOfStatementSign = null, 
                               TokenType? endOfBlockSign = null)
    {
        AST result;
        var isBlock = true; // kinda redundant, but still there, until a better solution is found

        switch (CurrentToken!.Value.Type)
        {
            // types or their possible alternatives
            case TokenType.Var:
            case TokenType.Void:
            case TokenType.Object:
            case TokenType.DataType:
                result = ParseType(scope, out isBlock);
                break;

            // block statements
            case TokenType.If:
            case TokenType.While:
            case TokenType.For:
                isBlock = true;
                var token3 = CurrentToken!.Value;
                AcceptAny();
                Accept(TokenType.OpeningParenthese);
                result = (GetType().GetMethod($"Parse{token3.Type}", BindingFlags.NonPublic | BindingFlags.Instance)!
                                   .Invoke(this, new object[] { scope, token3 }) as AST)!;
                break;

            // simple built-in functions (should be easier to expand in the future)
            case TokenType.Log:
            case TokenType.Input:
                var token = CurrentToken!.Value;
                AcceptAny();
                Accept(TokenType.OpeningParenthese);
                result = (Activator.CreateInstance(Type.GetType($"FAIL.ElementTree.{token.Type}")!, ParseCommand(scope, TokenType.ClosingParenthese), token) as AST)!;
                break;

            // simple statements following a keyword
            case TokenType.Return:
                var token2 = CurrentToken!.Value;
                AcceptAny();
                result = (Activator.CreateInstance(Type.GetType($"FAIL.ElementTree.{token2.Type}")!, ParseCommand(scope, endOfStatementSign), token2) as AST)!;
                break;

            // simple keywords without any additional information
            case TokenType.Continue:
            case TokenType.Break:
                result = (Activator.CreateInstance(Type.GetType($"FAIL.ElementTree.{CurrentToken!.Value.Type}")!, CurrentToken) as AST)!;
                AcceptAny();
                break;

            // all other stuff will be arithmetically parsed
            default:
                result = ParseTerm(scope);
                break;
        }

        if (endOfStatementSign is not null // command must have a endOfStatementSign
            && !isBlock
            && !(endOfBlockSign is not null && IsTypeOf(endOfBlockSign.Value))) // is ther an endOfBlockSign (like TokenType.ClosingParenthese, see e.g. 'testCommand' of an if)?
            Accept(endOfStatementSign!.Value);

        return result;
    }

    // the corresponding datatype will be invoked (currently just built-in types)
    protected static ElementTree.DataTypes.Object ParseObject(Type type, Token token) 
        => Activator.CreateInstance(type, token.Value, token);

    // this all parses arithmetically
    protected AST ParseTerm(Scope scope, Calculations calculations = (Calculations)15, AST? heap = null)
    {
        // just the desired categories will be parsed to allow custom rules like a hirarchy, standard (15) is just every category

        if (calculations.HasFlag(Calculations.Term)) heap = ParseTerm(scope, heap);
        if (calculations.HasFlag(Calculations.DotCalculations)) heap = ParseDotCalculation(scope, heap);
        if (calculations.HasFlag(Calculations.StrokeCalculations)) heap = ParseStrokeCalculation(scope, heap);
        if (calculations.HasFlag(Calculations.TestOperations)) heap = ParseTestOperations(scope, heap);

        return heap!;
    }
    protected AST? ParseTerm(Scope scope, AST? heap)
    {
        var token = CurrentToken;

        // add one level on top
        if (IsTypeOf(TokenType.OpeningParenthese))
        {
            AcceptAny();
            AST? subTerm;

            // negative number hack (0 - value -> substraction)
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

        // negative number hack (0 - value -> substraction)
        if (IsTypeOf(TokenType.StrokeCalculation) && HasValue("-"))
        {
            AcceptAny();

            return ParseTerm(scope,
                             Calculations.DotCalculations | Calculations.StrokeCalculations,
                             new Substraction(new ElementTree.DataTypes.Object(0),
                                              ParseTerm(scope, Calculations.Term)));
        }

        // currently a bit redundant code, until the type system is finally implemented
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

        // any non-string text
        if (IsTypeOf(TokenType.Identifier))
        {
            AcceptAny();

            if (IsTypeOf(TokenType.Assignment)) return ParseAssignment(scope, token!.Value); // test = 42;
            if (IsTypeOf(TokenType.SelfAssignment)) return ParseSelfAssignment(scope, token!.Value); // test += 42;
            if (IsTypeOf(TokenType.OpeningParenthese)) return ParseFunctionCall(scope, token); // Test();
            if (IsTypeOf(TokenType.IncrementalOperator)) return ParseIncrementalOperator(scope, token!.Value); // test++;
            return new Reference(scope, token);
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
            AcceptAny();
            var secondParameter = ParseTerm(scope, Calculations.Term);
            return ParseTerm(scope,
                             Calculations.DotCalculations | Calculations.StrokeCalculations | Calculations.TestOperations,
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
            AcceptAny();
            var secondParameter = ParseTerm(scope, Calculations.DotCalculations | Calculations.Term);
            return ParseTerm(scope,
                             Calculations.StrokeCalculations | Calculations.TestOperations,
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
            AcceptAny();
            var secondParameter = ParseTerm(scope, Calculations.StrokeCalculations | Calculations.DotCalculations | Calculations.Term);
            return ParseTerm(scope, 
                             Calculations.TestOperations, 
                             Activator.CreateInstance(TestOperatorMapper[GetValue(token)], heap, secondParameter, token));
        }

        return heap; // no test operator (possibly an error)
    }

    // everthing related to types (e.g. declarations)
    protected AST ParseType(Scope scope, out bool isBlock)
    {
        var type = CurrentToken!.Value.Type;
        var identifier = AcceptAny()!.Value;
        Accept(TokenType.Identifier);

        return IsTypeOf(TokenType.OpeningParenthese)
            ? ParseFunction(scope, out isBlock, type, identifier)
            : ParseVar(scope, out isBlock, type, identifier);
    }
    protected AST ParseVar(Scope scope, out bool isBlock, TokenType type, Token identifier)
    {
        isBlock = false;

        // identifier must be unique in scope (variables AND functions), local are superior to shared ones (identifier doesn't need to be unique)
        if (IsAssigned(scope, identifier.Value)) throw ExceptionCreator.AlreadyAssignedInScope(identifier!.Value.Value); 

        // unassigned variable (used in function parameters)
        if (!IsTypeOf(TokenType.Assignment)) return new Variable(identifier.Value, type.ToString(), token: identifier);

        // already assigned variable (get the value)
        Accept(TokenType.Assignment);
        return new Variable(identifier.Value, 
                            type.ToString(),
                            ParseCommand(scope), 
                            identifier);
    }
    protected AST ParseFunction(Scope scope, out bool isBlock, TokenType type, Token identifier)
    {
        isBlock = true;

        // identifier must be unique in scope (variables AND functions), local are superior to shared ones (identifier doesn't need to be unique)
        if (IsAssigned(scope, identifier.Value)) throw ExceptionCreator.AlreadyAssignedInScope(identifier!.Value.Value);

        // 'argList' may be empty
        Accept(TokenType.OpeningParenthese);
        var argList = ParseCommandList(TokenType.Separator, TokenType.ClosingParenthese);

        var body = ParseBody(argList.Commands, scope);

        // if there is a return type declared in front of the identifier, there has to be a return a the end (currently)
        if (type != TokenType.Void && body.Commands.Entries.Last() is not Return) 
            throw ExceptionCreator.FunctionMustReturnValue(identifier.Value);

        return new Function(identifier.Value, type.ToString(), argList, body, identifier);
    }

    // variable manipulations and function calls
    protected AST ParseAssignment(Scope scope, Token token)
    {
        Accept(TokenType.Assignment);
        return new Assignment(GetValidVariable(scope, token.Value, token), ParseCommand(scope));
    } // test = 42;
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
    } // test += 42;
    protected AST ParseFunctionCall(Scope scope, Token? token)
    {
        Accept(TokenType.OpeningParenthese);
        var parameters = ParseCommandList(TokenType.Separator, TokenType.ClosingParenthese, scope);
        return new FunctionCall(GetFunctionFromScope(scope, token!.Value.Value), parameters);
    } // Test();
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
    } // test++;

    // block statements
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
        var internalScope = new Scope(new()); // special scope for the iterator variable
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
            Accept(TokenType.OpeningBracket);
            return ParseCommandList(TokenType.EndOfStatement, TokenType.ClosingBracket, scopes);
        }
        else return new(new(new() { ParseCommand(new(new(), scopes)) }));
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
        if (GetFunctionFromScope(scope, name) is not null) return true; // function with the name found in scope
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
        if (TokenEnumerator is null) TokenEnumerator = Tokenizer.GetEnumerator(); // initialize the TokenEnumerator

        LastToken = CurrentToken; // LastToken could be useful, but there is also the TokenStack

        try
        {
            // get next token
            TokenEnumerator.MoveNext(); 
            CurrentToken = TokenEnumerator.Current;

            TokenStack.Add(CurrentToken);
        }
        catch (StopIterationException) // end of file reached
        {
            CurrentToken = null;
            return null;
        }

        return CurrentToken;
    }
    protected Token? Accept(TokenType expected)
    {
        if (TokenEnumerator is null) TokenEnumerator = Tokenizer.GetEnumerator(); // initialize the TokenEnumerator

        if (!expected.HasFlag(CurrentToken!.Value.Type)) throw ExceptionCreator.WrongToken(CurrentToken!.Value, expected);

        LastToken = CurrentToken; // LastToken could be useful, but there is also the TokenStack

        try
        {
            // get next token
            TokenEnumerator.MoveNext();
            CurrentToken = TokenEnumerator.Current;

            TokenStack.Add(CurrentToken);
        }
        catch (StopIterationException) // end of file reached
        {
            CurrentToken = null;
            return null;
        }

        return CurrentToken;
    }
}
