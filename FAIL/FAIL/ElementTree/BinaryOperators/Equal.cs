using FAIL.LanguageIntegration;

namespace FAIL.ElementTree.BinaryOperators;
internal class Equal : BinaryOperator
{
    public Equal(AST? firstParameter, AST? secondParameter, Token? token = null) : base(firstParameter, secondParameter, token)
    {
    }

    public override dynamic Calculate(dynamic firstParameter, dynamic secondParameter) => firstParameter == secondParameter;
}
