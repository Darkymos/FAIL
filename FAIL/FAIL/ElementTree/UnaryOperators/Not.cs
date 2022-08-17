using FAIL.LanguageIntegration;

namespace FAIL.ElementTree.UnaryOperators;
internal class Not : UnaryOperator
{
    public Not(AST parameter, Token? token = null) : base(parameter, token)
    {
    }

    public override DataTypes.Object Calculate(DataTypes.Object parameter) => new DataTypes.Boolean(!parameter.Call()!.Value);
}
