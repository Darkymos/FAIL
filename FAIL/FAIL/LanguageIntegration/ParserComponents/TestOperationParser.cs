using FAIL.ElementTree;
using FAIL.Metadata;

namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class TestOperationParser : IParserComponent
{
    private readonly TokenReader Reader;
    private readonly ArithmeticOperationParser ArithmeticOperationParser;

    private static readonly Dictionary<string, BinaryOperation> TestOperatorMapper = new()
    {
        { "==", BinaryOperation.Equal },
        { "!=", BinaryOperation.NotEqual },
        { ">=", BinaryOperation.GreaterThanOrEqual },
        { "<=", BinaryOperation.LessThanOrEqual },
        { ">", BinaryOperation.GreaterThan },
        { "<", BinaryOperation.LessThan },
    };


    public TestOperationParser(TokenReader reader, ArithmeticOperationParser arithmeticOperationParser)
    {
        Reader = reader;
        ArithmeticOperationParser = arithmeticOperationParser;
    }


    public AST? Parse(Scope scope, AST? heap = null)
    {
        if (Reader.IsEOT() || !Reader.IsTypeOf(TokenType.TestOperator)) return heap; // there is no test operator
        if (!TestOperatorMapper.ContainsKey(Reader.GetValue())) throw new NotImplementedException();

        var token = Reader.CurrentToken;
        _ = Reader.ConsumeCurrentToken();

        var secondParameter = ArithmeticOperationParser.Parse(scope, Calculations.TestOperations.GetAbove());

        return ArithmeticOperationParser.Parse(scope,
                                               Calculations.TestOperations.GetSelfAndBelow(),
                                               new BinaryOperator(TestOperatorMapper[Reader.GetValue(token)], heap, secondParameter, token));
    }
}
