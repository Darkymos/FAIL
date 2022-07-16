using FAIL.LanguageIntegration;

namespace FAIL.ElementTree.BinaryOperators;
internal class Addition : BinaryOperator
{
    public Addition(AST? firstParameter, AST? secondParameter, Token? token = null) : base(firstParameter, secondParameter, token)
    {
    }


    public override dynamic Calculate(dynamic firstParameter, dynamic secondParameter)
    {
        try
        {
            return firstParameter + secondParameter;
        }
        catch (InvalidOperationException)
        {
            return firstParameter.ToString() + secondParameter.ToString();
        }
    }
}
