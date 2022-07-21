using FAIL.Exceptions;
using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class Continue : AST
{
    public Continue(Token? token = null) : base(token)
    {
    }

    public override dynamic? Call() => throw new ContinueException();
}
