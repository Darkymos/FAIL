using FAIL.LanguageIntegration;

namespace FAIL.ElementTree.BinaryOperators;
internal class Or : BinaryOperator
{
    public Or(AST firstParameter, AST secondParameter, Token? token = null) : base(firstParameter, secondParameter, token)
    {
    }

    public override DataTypes.Object Calculate(DataTypes.Object firstParameter, DataTypes.Object secondParameter) => new DataTypes.Boolean(firstParameter.Value || secondParameter.Value);
}
