using FAIL.Exceptions;
using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class Break : AST
{
    public Break(Token? token = null) : base(token)
    {
    }

    public override dynamic? Call() => throw new BreakException();
}
