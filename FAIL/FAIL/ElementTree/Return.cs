using FAIL.Exceptions;
using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class Return : AST
{
    public AST? ReturnValue { get; }


    public Return(AST? returnValue, Token? token = null) : base(token) => ReturnValue = returnValue;


    public override DataTypes.Object? Call() => throw new ReturnException(ReturnValue?.Call());
    public override Type GetType() => ReturnValue!.GetType();
}
