namespace FAIL.Language_Integration;
internal class Token
{
    public TokenType Type { get; }
    public dynamic Value { get; }
    public uint Row { get; }
    public uint Column { get; } 
    public string FileName { get; }

    public Token(TokenType kind, dynamic value, uint row, uint column, string fileName)
    {
        Type = kind;
        Value = value;
        Row = row;
        Column = column;
        FileName = fileName;
    }
}
