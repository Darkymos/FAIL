using FAIL.Exceptions;
using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class Continue : AST
{
    public Continue(Token? token = null) : base(token)
    {
    }

    public override Instance? Call() => throw new ContinueException();
    public override Type GetType() => new("Undefined");
}
