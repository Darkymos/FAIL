using FAIL.ElementTree;
using FAIL.Metadata;

namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class DotCalculationParser : IParserComponent
{
    private readonly TokenReader Reader;
    private readonly ArithmeticOperationParser ArithmeticOperationParser;

    private static readonly Dictionary<string, BinaryOperation> DotOperatorMapper = new()
    {
        { "*", BinaryOperation.Multiplication },
        { "/", BinaryOperation.Division },
    };


    public DotCalculationParser(TokenReader reader, ArithmeticOperationParser arithmeticOperationParser)
    {
        Reader = reader;
        ArithmeticOperationParser = arithmeticOperationParser;
    }


    public AST? Parse(Scope scope, AST? heap = null)
    {
        if (Reader.IsEOT() || !Reader.IsTypeOf(TokenType.DotCalculation)) return heap; // there is no dot calculation
        if (!DotOperatorMapper.ContainsKey(Reader.GetValue())) throw new NotImplementedException();

        var token = Reader.CurrentToken;
        _ = Reader.ConsumeCurrentToken();

        var secondParameter = ArithmeticOperationParser.Parse(scope, Calculations.DotCalculations.GetAbove());

        return ArithmeticOperationParser.Parse(scope,
                                               Calculations.DotCalculations.GetSelfAndBelow(),
                                               new BinaryOperator(DotOperatorMapper[Reader.GetValue(token)], heap, secondParameter, token));
    }
}
