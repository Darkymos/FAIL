namespace FAIL.LanguageIntegration;

internal enum TokenType
{
    Number,

    StrokeCalculation,
    DotCalculation,

    EndOfStatement,
    String,
    OpeningParenthese,
    ClosingParenthese,
    Identifier,
    Assignment,
    OpeningBracket,
    ClosingBracket,
    Separator,
    Boolean,
    TestOperator,
    SelfAssignment,
    IncrementalOperator,

    Var,
    Void,
    Return,
    If,
    Else,
    While,
    Break,
    Continue,
    For,
    DataType,
    Conversion,
    LogicalOperator,
}
