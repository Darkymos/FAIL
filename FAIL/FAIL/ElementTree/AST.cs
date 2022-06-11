using FAIL.Language_Integration;

namespace FAIL.ElementTree;
internal abstract class AST
{
    public Token? Token { get; }

    public AST(Token? token = null)
    {
        Token = token;
    }

    public abstract object? Call();
}
