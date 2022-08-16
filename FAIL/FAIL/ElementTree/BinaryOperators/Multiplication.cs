using FAIL.LanguageIntegration;

namespace FAIL.ElementTree.BinaryOperators;
internal class Multiplication : BinaryOperator
{
    public Multiplication(AST firstParameter, AST secondParameter, Token? token = null) : base(firstParameter, secondParameter, token)
    {
    }

    public override DataTypes.Object Calculate(DataTypes.Object firstParameter, DataTypes.Object secondParameter)
        => Activator.CreateInstance(Type.GetUnderlyingType(GetCombinedType()), firstParameter.Value * secondParameter.Value, Token);
}
