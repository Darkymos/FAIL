using FAIL.LanguageIntegration;
using FAIL.Metadata;

namespace FAIL.ElementTree.UnaryOperators;
internal class Negation : UnaryOperator
{
    public Negation(AST parameter, Token? token = null) : base(parameter, token) => ReturnType = GetReturnType(UnaryOperation.Negation);

    public override DataTypes.Object Calculate(DataTypes.Object parameter)
        => (DataTypes.Object)Activator.CreateInstance(Type.GetUnderlyingType(ReturnType), new object?[] { -parameter.Value, Token })!;
}
