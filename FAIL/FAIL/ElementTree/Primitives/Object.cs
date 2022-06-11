using FAIL.Language_Integration;

namespace FAIL.ElementTree.Primitives;
internal class Object : AST
{
    public Object(Token? token = null) : base(token)
    {
    }

    public override object? Call() => Token!.Value;
}
