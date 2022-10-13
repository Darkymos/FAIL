using FAIL.ElementTree;

namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class TypeMemberParser : IParserComponent
{
    private readonly TokenReader Reader;
    private readonly CommandParser CommandParser;
    private readonly ArithmeticOperationParser ArithmeticOperationParser;


    public TypeMemberParser(TokenReader reader, CommandParser commandParser, ArithmeticOperationParser arithmeticOperationParser)
    {
        Reader = reader;
        CommandParser = commandParser;
        ArithmeticOperationParser = arithmeticOperationParser;
    }


    public AST Parse(Scope scope, Token token)
    {
        _ = Reader.ConsumeCurrentToken(TokenType.Accessor);

        var reference = new Reference(scope.GetValidVariable(token.Value, token), scope);

        return new InstanceCall(reference, ArithmeticOperationParser.Parse(reference.Variable.Call()!
                                                                    .GetValueAs<CustomClass>().Members.Commands, Calculations.Term));
    }
}
