using FAIL.ElementTree;
using FAIL.ElementTree.IntegratedFunctions;
using FAIL.ElementTree.Primitives;
using FAIL.ElementTree.Terms;

namespace FAIL.Language_Integration;
internal class Parser
{
    public Tokenizer Tokenizer { get; }

    private IEnumerator<Token> tokens;
    private Token? currentToken;

    public Parser(string filePath)
    {
        Tokenizer = new(filePath);
        tokens = Tokenizer.Tokens.GetEnumerator();

        AcceptAny();
    }

    public AST? Parse()
    {
        if (IsEOT()) return null;

        var ast = ParseCommandList(TokenType.EndOfLine);
        if (!IsEOT()) throw new InvalidOperationException();

        return ast;
    }

    private AST? ParseCommandList(TokenType breakSign, TokenType eotMarker = TokenType.None)
    {
        var cmdList = new List<AST?>();

        while (!IsEOT())
        {
            var ast = ParseCommand(breakSign, eotMarker);
            cmdList.Add(ast);
        }

        return new CommandList(cmdList);
    }
    private AST? ParseCommand(TokenType breakSign, TokenType eotMarker = TokenType.None)
    {
        if (currentToken!.Type == TokenType.FunctionKeyWord)
        {
            return ParseFunctionKeyword();
        }
        else if (currentToken!.Type == TokenType.Int)
        {
            return ParseTerm();
        }
        else throw new NotSupportedException();
    }

    private AST? ParseFunctionKeyword()
    {
        if (currentToken!.Value == "WriteLine")
        {
            var token = Accept(TokenType.FunctionKeyWord);
            Accept(TokenType.LeftParenthesis);
            var cmd = ParseCommand(TokenType.None);
            Accept(TokenType.RightParenthesis);
            Accept(TokenType.EndOfLine);

            return new WriteLine(currentToken, cmd);
        }
        else return null;
    }
    private AST? ParseTerm()
    {
        var token = currentToken;
        var operation = Accept(TokenType.DotCalculation | TokenType.StrokeCalculation);

        if (operation!.Type == TokenType.DotCalculation)
        {
            if (operation!.Value == "*") return new Multiplication(new Integer(token), ParseTerm());
        }
        else if (operation!.Type == TokenType.StrokeCalculation)
        {

        }
        else throw new InvalidOperationException();
    }

    private bool IsEOT() => currentToken is null;
    private bool IsTypeOf(TokenType type) => currentToken!.Type == type;

    private Token? AcceptAny()
    {
        tokens.MoveNext();
        currentToken = tokens.Current;

        return currentToken;
    }
    private Token? Accept(TokenType expected)
    {
        if (!expected.HasFlag(currentToken!.Type)) throw new InvalidOperationException($"Invalid token '{currentToken.Value}' in file '{currentToken.FileName}' line {currentToken.Row}, column {currentToken.Column}! \n'{expected}' expected.");

        tokens.MoveNext();
        currentToken = tokens.Current;

        return currentToken;
    }
}
