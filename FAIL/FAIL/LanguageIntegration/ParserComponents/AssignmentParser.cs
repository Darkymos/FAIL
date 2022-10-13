using FAIL.ElementTree;

namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class AssignmentParser : IParserComponent
{
    private readonly TokenReader Reader;
    private readonly CommandParser CommandParser;


    public AssignmentParser(TokenReader reader, CommandParser commandParser)
    {
        Reader = reader;
        CommandParser = commandParser;
    }


    public AST Parse(Scope scope, Token token)
    {
        _ = Reader.ConsumeCurrentToken(TokenType.Assignment);

        var variable = scope.GetValidVariable(token.Value, token);
        var newValue = CommandParser.Parse(scope);

        // TODO type check

        return new Assignment(variable, newValue, token);
    }
}
