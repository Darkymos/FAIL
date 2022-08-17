using FAIL.LanguageIntegration;

namespace FAIL.ElementTree.UnaryOperators;
internal class Negation : UnaryOperator
{
    public Negation(AST parameter, Token? token = null) : base(parameter, token)
    {
    }

    public override DataTypes.Object Calculate(DataTypes.Object parameter) 
        => (DataTypes.Object)Activator.CreateInstance(Type.GetUnderlyingType(parameter.Call()!.GetType()), args: new object?[] { -parameter.Call()!.Value, null })!;
}
