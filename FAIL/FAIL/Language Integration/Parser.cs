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

    protected AST? ParseCommandList(TokenType endOfStatementSign, TokenType? endOfBlockSign = null)
    {
        var commands = new List<AST?>();

        while (!IsEOT() && (endOfBlockSign is null || !IsTypeOf(endOfBlockSign.Value))) 
            commands.Add(ParseCommand(endOfStatementSign));

        return new CommandList(commands);
    }
    protected AST? ParseCommand(TokenType endOfStatementSign)
    {
        AST? result;

        if (IsTypeOf(TokenType.KeyWord))
        {
            switch ((KeyWord)CurrentToken!.Value.Value)
            {
                case KeyWord.Log:
                    result = ParseLog();
                    break;
                default:
                    throw new NotImplementedException();
            };
        }
        else result = ParseStrokeCalculation();

        Accept(endOfStatementSign);
        return result;
    }

    protected Element_Tree.DataTypes.Object? ParseObject() => new(CurrentToken);

    protected AST? ParseStrokeCalculation() => ParseStrokeCalculation(ParseMultiplication());
    protected AST? ParseStrokeCalculation(AST? heap)
    {
        if (IsEOT() || !IsTypeOf(TokenType.StrokeCalculation)) return heap;

        var token = CurrentToken;
        AcceptAny();
        var secondParameter = ParseMultiplication();

        return HasValue(token, "+")
            ? ParseStrokeCalculation(new Addition(heap, secondParameter, token))
            : ParseStrokeCalculation(new Substraction(heap, secondParameter, token));
    }
    protected AST? ParseMultiplication() => ParseMultiplication(ParseTerm());
    protected AST? ParseMultiplication(AST? heap)
    {
        if (IsEOT()) return heap;

        var token = CurrentToken;

        if (HasValue(token, "*"))
        {
            AcceptAny();
            return ParseMultiplication(new Multiplication(heap, ParseTerm(), token));
        }
        else if (HasValue(token, "/"))
        {
            AcceptAny();
            return ParseMultiplication(new Division(heap, ParseTerm(), token));
        }
        else return heap;
    }
    protected AST? ParseTerm()
    {
        if (IsTypeOf(TokenType.OpeningParenthese))
        {
            AcceptAny();
            AST? subTerm;

            if (IsTypeOf(TokenType.StrokeCalculation) && HasValue("-"))
            {
                AcceptAny();
                subTerm = ParseStrokeCalculation(ParseMultiplication(new Substraction(
                                                                         new Element_Tree.DataTypes.Object(
                                                                             new(TokenType.Number, 0, 0, 0, "None")),
                                                                         ParseTerm())));
            }
            else subTerm = ParseStrokeCalculation();

            Accept(TokenType.ClosingParenthese);
            return subTerm;
        }

        if (IsTypeOf(TokenType.StrokeCalculation) && HasValue("-"))
        {
            AcceptAny();
            return ParseStrokeCalculation(ParseMultiplication(new Substraction(
                                                                  new Element_Tree.DataTypes.Object(
                                                                      new(TokenType.Number, 0, 0, 0, "None")), 
                                                                  ParseTerm())));
        }

        if (IsTypeOf(TokenType.Number) || IsTypeOf(TokenType.String))
        {
            var token = CurrentToken;
            AcceptAny();
            return new Element_Tree.DataTypes.Object(token);
        }

        throw ExceptionCreator.UnexpectedToken(CurrentToken!.Value);
    }

    protected AST? ParseLog()
    {
        AcceptAny();
        Accept(TokenType.OpeningParenthese);
        return new Log(ParseCommand(TokenType.ClosingParenthese));
    }


    private bool IsEOT() => CurrentToken is null;
    private bool IsTypeOf(TokenType type) => CurrentToken!.Value.Type == type;
    private bool HasValue(dynamic value) => CurrentToken!.Value.Value == value;
    private static bool HasValue(Token? token, dynamic value) => token!.Value.Value.GetType() == value.GetType() && token!.Value.Value == value;

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
