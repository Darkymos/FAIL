namespace FAIL.Language_Integration;
[Flags]
internal enum TokenType
{
    None = -1,
    Any = 0,
    String = 1,
    Int = 2,
    Float = 4,
    LeftParenthesis = 8,
    RightParenthesis = 16,
    EndOfLine = 128,
    KeyWord = 256,
    FunctionKeyWord = 512,
    EndOfText = 1024,
    Operator = 2048,
}
