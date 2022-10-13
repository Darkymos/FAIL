using FAIL.ElementTree;
using FAIL.Metadata;

namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class TermParser : IParserComponent
{
    private readonly TokenReader Reader;
    private readonly ArithmeticOperationParser ArithmeticOperationParser;
    private readonly BuiltInClassParser BuiltInClassParser;
    private readonly IdentifierParser IdentifierParser;


    public TermParser(TokenReader reader, ArithmeticOperationParser arithmeticOperationParser, 
                      BuiltInClassParser builtInClassParser, IdentifierParser identifierParser)
    {
        Reader = reader;
        ArithmeticOperationParser = arithmeticOperationParser;
        BuiltInClassParser = builtInClassParser;
        IdentifierParser = identifierParser;
    }


    public AST? Parse(Scope scope, AST? heap = null) => Reader.CurrentToken!.Value.Type switch
    {
        TokenType.OpeningParenthese => ParseNested(scope),
        TokenType.StrokeCalculation => ParseStrokeCalculation(scope),
        TokenType.UnaryLogicalOperator => ParseUnaryLogicalOperator(scope),
        TokenType.Number => ParseNumber(Reader.CurrentToken),
        TokenType.String => ParseString(Reader.CurrentToken),
        TokenType.Char => ParseChar(Reader.CurrentToken),
        TokenType.Boolean => ParseBoolean(Reader.CurrentToken),
        TokenType.Identifier => IdentifierParser.Parse(scope, Reader.CurrentToken),
        _ => heap
    };

    private AST ParseNested(Scope scope)
    {
        _ = Reader.ConsumeCurrentToken(TokenType.OpeningParenthese);
        var subTerm = ArithmeticOperationParser.Parse(scope);
        _ = Reader.ConsumeCurrentToken(TokenType.ClosingParenthese);

        return subTerm;
    }
    private AST ParseStrokeCalculation(Scope scope)
    {
        var operatorToken = Reader.CurrentToken;
        _ = Reader.ConsumeCurrentToken(TokenType.StrokeCalculation);

        return Reader.HasValue(operatorToken, "+")
            ? ArithmeticOperationParser.Parse(scope, ArithmeticOperationParser.Parse(scope, Calculations.Term))
            : ArithmeticOperationParser.Parse(scope, new UnaryOperator(UnaryOperation.Negation,
                                                                       ArithmeticOperationParser.Parse(scope, Calculations.Term)));
    }
    private AST ParseUnaryLogicalOperator(Scope scope)
    {
        _ = Reader.ConsumeCurrentToken(TokenType.LogicalOperator);

        return ArithmeticOperationParser.Parse(scope, new UnaryOperator(UnaryOperation.Not,
                                                                        ArithmeticOperationParser.Parse(scope, Calculations.Term)));
    }
    private AST ParseNumber(Token? token)
    {
        _ = Reader.ConsumeCurrentToken();

        if (token!.Value.Value is int) return BuiltInClassParser.Parse(new("Integer"), token);
        if (token!.Value.Value is double) return BuiltInClassParser.Parse(new("Double"), token);

        throw ExceptionCreator.InvalidToken(Reader.CurrentToken!.Value, TokenType.Number);
    }
    private AST ParseString(Token? token)
    {
        _ = Reader.ConsumeCurrentToken();
        return BuiltInClassParser.Parse(new("String"), token);
    }
    private AST ParseChar(Token? token)
    {
        _ = Reader.ConsumeCurrentToken();
        return BuiltInClassParser.Parse(new("Char"), token);
    }
    private AST ParseBoolean(Token? token)
    {
        _ = Reader.ConsumeCurrentToken();
        return BuiltInClassParser.Parse(new("Boolean"), token);
    }
}
