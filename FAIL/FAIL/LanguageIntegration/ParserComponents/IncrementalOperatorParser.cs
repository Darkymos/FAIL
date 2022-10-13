using FAIL.BuiltIn.DataTypes;
using FAIL.ElementTree;
using FAIL.Metadata;

namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class IncrementalOperatorParser : IParserComponent
{
    private readonly TokenReader Reader;
    private readonly CommandParser CommandParser;

    private static readonly Dictionary<string, BinaryOperation> IncrementalOperatorMapper = new()
    {
        { "++", BinaryOperation.Addition },
        { "--", BinaryOperation.Substraction },
    };


    public IncrementalOperatorParser(TokenReader reader, CommandParser commandParser)
    {
        Reader = reader;
        CommandParser = commandParser;
    }


    public AST Parse(Scope scope, Token token)
    {
        var op = Reader.CurrentToken;
        _ = Reader.ConsumeCurrentToken();

        var variable = scope.GetValidVariable(token.Value, token);

        return new Assignment(variable,
                              new BinaryOperator(IncrementalOperatorMapper[Reader.GetValue(op)], variable, new Instance(Integer.Type, 1), token));
    }
}
