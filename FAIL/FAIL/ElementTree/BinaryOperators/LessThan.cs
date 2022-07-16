using FAIL.LanguageIntegration;

namespace FAIL.ElementTree.BinaryOperators;
internal class LessThan : BinaryOperator
{
    public LessThan(AST? firstParameter, AST? secondParameter, Token? token = null) : base(firstParameter, secondParameter, token)
    {
    }

    public override dynamic Calculate(dynamic firstParameter, dynamic secondParameter) => firstParameter < secondParameter;
}
