using FAIL.Language_Integration;

namespace FAIL.Element_Tree.DataTypes;
internal class Object : AST
{
    public Object(Token? token = null) : base(token)
    {
    }

    public override dynamic? Call() => Token?.Value;
    public override string ToString() => $"{nameof(Object)}";
}
