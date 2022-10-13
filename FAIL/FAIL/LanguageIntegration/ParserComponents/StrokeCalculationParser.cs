using FAIL.ElementTree;
using FAIL.Metadata;

namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class StrokeCalculationParser : IParserComponent
{
    private readonly TokenReader Reader;
    private readonly ArithmeticOperationParser ArithmeticOperationParser;

    private static readonly Dictionary<string, BinaryOperation> StrokeOperatorMapper = new()
    {
        { "+", BinaryOperation.Addition },
        { "-", BinaryOperation.Substraction },
    };


    public StrokeCalculationParser(TokenReader reader, ArithmeticOperationParser arithmeticOperationParser)
    {
        Reader = reader;
        ArithmeticOperationParser = arithmeticOperationParser;
    }


    public AST? Parse(Scope scope, AST? heap = null)
    {
        if (Reader.IsEOT() || !Reader.IsTypeOf(TokenType.StrokeCalculation)) return heap; // there is not stroke calculation
        if (!StrokeOperatorMapper.ContainsKey(Reader.GetValue())) throw new NotImplementedException();

        var token = Reader.CurrentToken;
        _ = Reader.ConsumeCurrentToken();

        var secondParameter = ArithmeticOperationParser.Parse(scope, Calculations.StrokeCalculations.GetAbove());

        return ArithmeticOperationParser.Parse(scope,
                                               Calculations.StrokeCalculations.GetSelfAndBelow(),
                                               new BinaryOperator(StrokeOperatorMapper[Reader.GetValue(token)], heap, secondParameter, token));
    }
}
