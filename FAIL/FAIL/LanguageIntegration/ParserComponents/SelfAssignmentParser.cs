using FAIL.ElementTree;
using FAIL.Metadata;

namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class SelfAssignmentParser : IParserComponent
{
    private readonly TokenReader Reader;
    private readonly CommandParser CommandParser;

    private static readonly Dictionary<string, BinaryOperation> SelfAssignmentOperatorMapper = new()
    {
        { "+=", BinaryOperation.Addition },
        { "-=", BinaryOperation.Substraction },
        { "*=", BinaryOperation.Multiplication },
        { "/=", BinaryOperation.Division },
    };


    public SelfAssignmentParser(TokenReader reader, CommandParser commandParser)
    {
        Reader = reader;
        CommandParser = commandParser;
    }


    public AST Parse(Scope scope, Token token)
    {
        var op = Reader.CurrentToken;
        _ = Reader.ConsumeCurrentToken(TokenType.SelfAssignment);

        var variable = (Variable)scope.GetValidVariable(token.Value, token);
        var newValue = CommandParser.Parse(scope);

        // TODO type check

        return new Assignment(variable,
                              new BinaryOperator(SelfAssignmentOperatorMapper[Reader.GetValue(op)], variable, newValue, token));
    }
}
