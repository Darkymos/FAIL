using FAIL.ElementTree;
using FAIL.Metadata;

namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class LogicalOperationParser : IParserComponent
{
    private readonly TokenReader Reader;
    private readonly ArithmeticOperationParser ArithmeticOperationParser;

    private static readonly Dictionary<string, BinaryOperation> LogicalOperatorMapper = new()
    {
        { "or", BinaryOperation.Or },
        { "and", BinaryOperation.And },
        { "||", BinaryOperation.Or },
        { "&&", BinaryOperation.And },
    };


    public LogicalOperationParser(TokenReader reader, ArithmeticOperationParser arithmeticOperationParser)
    {
        Reader = reader;
        ArithmeticOperationParser = arithmeticOperationParser;
    }


    public AST? Parse(Scope scope, AST? heap = null)
    {
        if (Reader.IsEOT() || !Reader.IsTypeOf(TokenType.LogicalOperator)) return heap; // there is no logical operator
        if (!LogicalOperatorMapper.ContainsKey(Reader.GetValue())) throw new NotImplementedException();

        var token = Reader.CurrentToken;
        _ = Reader.ConsumeCurrentToken();

        var secondParameter = ArithmeticOperationParser.Parse(scope, Calculations.LogicalOperations.GetAbove());

        return ArithmeticOperationParser.Parse(scope,
                                               Calculations.LogicalOperations.GetSelfAndBelow(),
                                               new BinaryOperator(LogicalOperatorMapper[Reader.GetValue(token)], heap, secondParameter, token));
    }
}
