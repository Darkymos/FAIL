using FAIL.LanguageIntegration;
using FAIL.Metadata;

namespace FAIL.ElementTree.BinaryOperators;
internal class LessThanOrEqual : BinaryOperator
{
    public LessThanOrEqual(AST firstParameter, AST secondParameter, Token? token = null) : base(firstParameter, secondParameter, token) 
        => ReturnType = GetReturnType(BinaryOperation.LessThanOrEqual);

    public override DataTypes.Object Calculate(DataTypes.Object firstParameter, DataTypes.Object secondParameter)
        => (DataTypes.Object)Activator.CreateInstance(Type.GetUnderlyingType(ReturnType),
                                                      args: new object?[] { firstParameter.Value <= secondParameter.Value, Token })!;
}
