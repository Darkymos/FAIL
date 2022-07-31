using FAIL.Exceptions;
using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class Break : AST
{
    public Break(Token? token = null) : base(token)
    {
    }

    public override DataTypes.Object? Call() => throw new BreakException();
    public override Type GetType() => new("Undefined");
}
