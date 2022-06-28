using FAIL.Language_Integration;

namespace FAIL.Element_Tree;
internal abstract class AST
{
    public Token? Token { get; init; }


    public AST(Token? token = null) => Token = token;


    public abstract dynamic? Call();
    public override string ToString() => $"{nameof(AST)}";
}
