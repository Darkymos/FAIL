using FAIL.Language_Integration;

namespace FAIL.Element_Tree.BinaryOperators;
internal class GreaterThanOrEqual : BinaryOperator
{
    public GreaterThanOrEqual(AST? firstParameter, AST? secondParameter, Token? token = null) : base(firstParameter, secondParameter, token)
    {
    }

    public override dynamic Calculate(dynamic firstParameter, dynamic secondParameter) => firstParameter >= secondParameter;
}
