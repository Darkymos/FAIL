using FAIL.LanguageIntegration;

namespace FAIL.ElementTree.BinaryOperators;
internal class GreaterThan : BinaryOperator
{
    public GreaterThan(AST? firstParameter, AST? secondParameter, Token? token = null) : base(firstParameter, secondParameter, token)
    {
    }

    public override dynamic Calculate(dynamic firstParameter, dynamic secondParameter) => firstParameter > secondParameter;
}
