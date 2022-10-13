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
    Char,
    Accessor,
    Class,
    New,
    Init,
    UnaryLogicalOperator,
}

internal static class TokenTypeTranslator
{
    public const int LONGEST_OPERATOR = 2; // just to avoid unnessesary looping in Tokenizer.CheckForOperator()

    public static TokenType? GetOperator(string raw) => raw switch
    {
        // Operators
        "==" or "!=" or ">=" or "<=" or ">" or "<" => TokenType.TestOperator,
        "||" or "&&" => TokenType.LogicalOperator,
        "!" => TokenType.UnaryLogicalOperator,
        "+=" or "-=" or "*=" or "/=" => TokenType.SelfAssignment,
        "++" or "--" => TokenType.IncrementalOperator,
        "+" or "-" => TokenType.StrokeCalculation,
        "*" or "/" => TokenType.DotCalculation,

        // Annotation Signs
        "," => TokenType.Separator,
        ";" => TokenType.EndOfStatement,
        "=" => TokenType.Assignment,
        "." => TokenType.Accessor,

        // Delimiters
        "(" => TokenType.OpeningParenthese,
        ")" => TokenType.ClosingParenthese,
        "{" => TokenType.OpeningBracket,
        "}" => TokenType.ClosingBracket,

        // Not found
        _ => null,
    };
    public static TokenType? GetKeyWord(string raw) => raw switch
    {
        // Special types
        "var" => TokenType.Var,
        "void" => TokenType.Void,

        // Data types
        "object" or "int" or "double" or "string" or "char" or "bool" => TokenType.DataType,

        // OOP
        "class" => TokenType.Class,
        "new" => TokenType.New,
        "init" => TokenType.Init,

        // Decisions
        "return" => TokenType.Return,
        "if" => TokenType.If,
        "else" => TokenType.Else,

        // Loops
        "while" => TokenType.While,
        "break" => TokenType.Break,
        "continue" => TokenType.Continue,
        "for" => TokenType.For,

        // Conversions
        "as" => TokenType.Conversion,

        // Logical operators
        "or" or "and" => TokenType.LogicalOperator,
        "not" => TokenType.UnaryLogicalOperator,

        // Not found
        _ => null,
    };
}
