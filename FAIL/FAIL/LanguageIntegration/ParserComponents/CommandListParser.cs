using FAIL.ElementTree;

namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class CommandListParser : IParserComponent
{
    private readonly TokenReader Reader;
    private readonly CommandParser CommandParser;


    public CommandListParser(TokenReader reader, CommandParser commandParser)
    {
        Reader = reader;
        CommandParser = commandParser;
    }


    public CommandList Parse(TokenType endOfStatementSign, TokenType? endOfBlockSign = null, params Scope[] shared)
    {
        var commands = new Scope(new(), shared); // owned scope

        while (!IsEnd(endOfBlockSign)) commands.Add(CommandParser.Parse(commands, endOfStatementSign, endOfBlockSign)!);

        // most often TokenType.ClosingBracket
        if (endOfBlockSign is not null) _ = Reader!.ConsumeCurrentToken(endOfBlockSign.Value);

        return new(commands);
    }
    public CommandList Parse(params Scope[] scopes) => Parse(true, scopes);
    public CommandList Parse(bool allowSingleStatement, params Scope[] scopes)
    {
        if (Reader.IsTypeOf(TokenType.OpeningBracket))
        {
            _ = Reader.ConsumeCurrentToken(TokenType.OpeningBracket);
            return Parse(TokenType.EndOfStatement, TokenType.ClosingBracket, scopes);
        }

        return allowSingleStatement
            ? new(new Scope(new List<AST>() { CommandParser.Parse(new Scope(scopes)) }))
            : throw ExceptionCreator.InvalidToken(Reader.CurrentToken!.Value, TokenType.OpeningBracket);
    }

    private bool IsEnd(TokenType? endOfBlockSign)
    {
        if (Reader!.IsEOT() && endOfBlockSign is not null)
            throw ExceptionCreator.UnexpectedToken(Reader!.CurrentToken!.Value);

        var endOfBlock = endOfBlockSign is not null && Reader!.IsTypeOf(endOfBlockSign.Value);
        var topLevel = Reader!.IsEOT();

        return endOfBlock || topLevel;
    }
}
