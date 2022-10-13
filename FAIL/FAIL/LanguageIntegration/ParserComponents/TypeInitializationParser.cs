using FAIL.ElementTree;

namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class TypeInitializationParser : IParserComponent
{
    private readonly TokenReader Reader;
    private readonly CommandListParser CommandListParser;


    public TypeInitializationParser(TokenReader reader, CommandListParser commandListParser)
    {
        Reader = reader;
        CommandListParser = commandListParser;
    }


    public AST Parse(Scope scope)
    {
        var typeName = Reader.ConsumeCurrentToken(TokenType.New);
        _ = Reader.ConsumeCurrentToken(TokenType.Identifier);

        _ = Reader.ConsumeCurrentToken(TokenType.OpeningParenthese);
        var parameters = CommandListParser.Parse(TokenType.Separator, TokenType.ClosingParenthese, scope);

        return new Instance(new ElementTree.Type(typeName!.Value.Value), scope, parameters);
    }
}
