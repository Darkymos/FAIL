using FAIL.ElementTree;

namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class ArithmeticOperationParser : IParserComponent
{
    private readonly TokenReader Reader;
    private readonly TermParser TermParser;


    public ArithmeticOperationParser(TokenReader reader, TermParser termParser)
    {
        Reader = reader;
        TermParser = termParser;
    }


    public AST Parse(Scope scope, AST? heap = null) => Parse(scope, CalculationsExtensions.All, heap);
    public AST Parse(Scope scope, Calculations calculations, AST? heap = null)
    {
        if (calculations.HasFlag(Calculations.Term)) heap = TermParser.Parse(scope, heap);
        if (calculations.HasFlag(Calculations.DotCalculations)) heap = new DotCalculationParser(Reader, this).Parse(scope, heap);
        if (calculations.HasFlag(Calculations.StrokeCalculations)) heap = new StrokeCalculationParser(Reader, this).Parse(scope, heap);
        if (calculations.HasFlag(Calculations.TestOperations)) heap = new TestOperationParser(Reader, this).Parse(scope, heap);
        if (calculations.HasFlag(Calculations.LogicalOperations)) heap = new LogicalOperationParser(Reader, this).Parse(scope, heap);
        if (calculations.HasFlag(Calculations.Conversions)) heap = ParseConversion(heap);

        return heap!;
    }

    private AST? ParseConversion(AST? heap)
    {
        if (Reader.IsEOT() || !Reader.IsTypeOf(TokenType.Conversion)) return heap; // there is no conversion

        var token = Reader.CurrentToken;
        _ = Reader.ConsumeCurrentToken();

        var newType = new ElementTree.Type(Reader.GetValue());
        _ = Reader.ConsumeCurrentToken(TokenType.DataType);

        return new TypeConversion(heap!, newType, token);
    }
}
