using FAIL.BuiltIn;
using FAIL.ElementTree;

namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class FunctionCallParser : IParserComponent
{
    private readonly TokenReader Reader;
    private readonly CommandParser CommandParser;
    private readonly CommandListParser CommandListParser;


    public FunctionCallParser(TokenReader reader, CommandParser commandParser, CommandListParser commandListParser)
    {
        Reader = reader;
        CommandParser = commandParser;
        CommandListParser = commandListParser;
    }


    public AST Parse(Scope scope, Token token)
    {
        _ = Reader.ConsumeCurrentToken(TokenType.OpeningParenthese);
        var parameters = CommandListParser.Parse(TokenType.Separator, TokenType.ClosingParenthese, scope);

        return BuiltInFunctions.Functions.ContainsKey(token!.Value.Value)
            ? new BuiltInFunctionCall(token!.Value.Value, parameters)
            : new FunctionCall(scope.GetFunctionFromScope(token!.Value.Value), parameters, token);
    }
}
