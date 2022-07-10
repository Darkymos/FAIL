using FAIL.Element_Tree;
using FAIL.Element_Tree.Operators;
using FAIL.Exceptions;

namespace FAIL.Language_Integration;
internal class Parser
{
    public Tokenizer Tokenizer { get; }

    private IEnumerator<Token>? TokenEnumerator;
    private Token? CurrentToken = null;


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
            KeyWord.Var => ParseVar(scope),
            KeyWord.Void => ParseFunction(scope, out endOfStatementSignRequired),
            _ => throw new NotImplementedException(),
        };
        else result = ParseStrokeCalculation(scope);

        if (acceptEndOfStatementSign 
            && endOfStatementSignRequired 
            && !IsEOT() 
            && !(endOfBlockSign is not null && IsTypeOf(endOfBlockSign.Value))) 
            Accept(endOfStatementSign);

        return result;
    }

    protected Element_Tree.DataTypes.Object? ParseObject() => new(CurrentToken);

    protected AST? ParseStrokeCalculation(Scope scope) => ParseStrokeCalculation(scope, ParseMultiplication(scope));
    protected AST? ParseStrokeCalculation(Scope scope, AST? heap)
    {
        if (IsEOT() || !IsTypeOf(TokenType.StrokeCalculation)) return heap;

        var token = CurrentToken;
        AcceptAny();
        var secondParameter = ParseMultiplication(scope);

        return HasValue(token, "+")
            ? ParseStrokeCalculation(scope, new Addition(heap, secondParameter, token))
            : ParseStrokeCalculation(scope, new Substraction(heap, secondParameter, token));
    }
    protected AST? ParseMultiplication(Scope scope) => ParseMultiplication(scope, ParseTerm(scope));
    protected AST? ParseMultiplication(Scope scope, AST? heap)
    {
        if (IsEOT()) return heap;

        var token = CurrentToken;

        if (HasValue(token, "*"))
        {
            AcceptAny();
            return ParseMultiplication(scope, new Multiplication(heap, ParseTerm(scope), token));
        }
        else if (HasValue(token, "/"))
        {
            AcceptAny();
            return ParseMultiplication(scope, new Division(heap, ParseTerm(scope), token));
        }
        else return heap;
    }
    protected AST? ParseTerm(Scope scope)
    {
        if (IsTypeOf(TokenType.OpeningParenthese))
        {
            AcceptAny();
            AST? subTerm;

            if (IsTypeOf(TokenType.StrokeCalculation) && HasValue("-"))
            {
                AcceptAny();
                subTerm = ParseStrokeCalculation(scope,
                                                 ParseMultiplication(scope,
                                                                     new Substraction(
                                                                         new Element_Tree.DataTypes.Object(
                                                                             new(TokenType.Number, 0, 0, 0, "None")),
                                                                         ParseTerm(scope))));
            }
            else subTerm = ParseStrokeCalculation(scope);

            Accept(TokenType.ClosingParenthese);
            return subTerm;
        }

        if (IsTypeOf(TokenType.StrokeCalculation) && HasValue("-"))
        {
            AcceptAny();
            return ParseStrokeCalculation(scope,
                                          ParseMultiplication(scope,
                                                              new Substraction(
                                                                  new Element_Tree.DataTypes.Object(
                                                                      new(TokenType.Number, 0, 0, 0, "None")), 
                                                                  ParseTerm(scope))));
        }

        if (IsTypeOf(TokenType.Number) || IsTypeOf(TokenType.String))
        {
            var token = CurrentToken;
            AcceptAny();
            return new Element_Tree.DataTypes.Object(token);
        }

        if (IsTypeOf(TokenType.Identifier))
        {
            var token = CurrentToken;
            AcceptAny();

            if (IsTypeOf(TokenType.Assignment)) return ParseAssignment(scope, token!.Value);

            if (IsTypeOf(TokenType.OpeningParenthese))
            {
                Accept(TokenType.OpeningParenthese);
                var parameters = ParseCommandList(TokenType.Separator, TokenType.ClosingParenthese, scope);
                return new FunctionCall(GetFunctionFromScope(scope, token!.Value.Value), parameters);
            }

            return new Reference(scope, token);
        }

        throw ExceptionCreator.UnexpectedToken(CurrentToken!.Value);
    }

    protected AST? ParseLog(Scope scope)
    {
        var logToken = CurrentToken;

        AcceptAny();
        Accept(TokenType.OpeningParenthese);

        return new Log(ParseCommand(scope, TokenType.ClosingParenthese), logToken);
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

        return new Function(identifier!.Value.Value, returnType!.Value.Value.ToString(), argList, cmdList, identifier);
    }
    protected AST? ParseAssignment(Scope scope, Token token)
    {
        Accept(TokenType.Assignment);

        if (!IsAssigned(scope, token.Value)) throw ExceptionCreator.NotAssignedInScope(CurrentToken!.Value.Value);
        if (GetVariableFromScope(scope, token.Value) is null) throw ExceptionCreator.VariableExpected();

        return new Assignment(GetVariableFromScope(scope, token.Value), ParseStrokeCalculation(scope));
    }


    private bool IsEOT() => CurrentToken is null;
    private bool IsTypeOf(TokenType type) => CurrentToken!.Value.Type == type;
    private bool HasValue(dynamic value) => CurrentToken!.Value.Value == value;
    private static bool HasValue(Token? token, dynamic value) => token!.Value.Value.GetType() == value.GetType() && token!.Value.Value == value;

    private static bool IsAssigned(Scope scope, string name)
    {
        if (GetVariableFromScope(scope, name) is not null) return true;
        if (GetFunctionFromScope(scope, name) is not null) return true;
        return false;
    }
    public static Variable? GetVariableFromScope(Scope scope, string name)
        => scope.Search(x => x is Variable variable && variable.Name == name) as Variable;
    public static Function? GetFunctionFromScope(Scope scope, string name)
        => scope.Search(x => x is Function function && function.Name == name) as Function;

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
