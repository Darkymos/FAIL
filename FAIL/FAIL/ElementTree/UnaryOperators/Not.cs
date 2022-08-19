using FAIL.LanguageIntegration;
using FAIL.Metadata;

namespace FAIL.ElementTree.UnaryOperators;
internal class Not : UnaryOperator
{
    public Not(AST parameter, Token? token = null) : base(parameter, token) => ReturnType = GetReturnType(UnaryOperation.Not);

    public override DataTypes.Object Calculate(DataTypes.Object parameter)
        => (DataTypes.Object)Activator.CreateInstance(Type.GetUnderlyingType(ReturnType), new object?[] { !parameter.Value, Token })!;
}
