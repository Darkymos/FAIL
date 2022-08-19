using FAIL.LanguageIntegration;
using FAIL.Metadata;

namespace FAIL.ElementTree.BinaryOperators;
internal class And : BinaryOperator
{
    public And(AST firstParameter, AST secondParameter, Token? token = null) : base(firstParameter, secondParameter, token) 
        => ReturnType = GetReturnType(BinaryOperation.And);

    public override DataTypes.Object Calculate(DataTypes.Object firstParameter, DataTypes.Object secondParameter)
        => (DataTypes.Object)Activator.CreateInstance(Type.GetUnderlyingType(ReturnType),
                                                      args: new object?[] { firstParameter.Value && secondParameter.Value, Token })!;
}
