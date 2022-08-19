using FAIL.LanguageIntegration;
using FAIL.Metadata;

namespace FAIL.ElementTree.BinaryOperators;
internal class LessThan : BinaryOperator
{
    public LessThan(AST firstParameter, AST secondParameter, Token? token = null) : base(firstParameter, secondParameter, token) 
        => ReturnType = GetReturnType(BinaryOperation.LessThan);

    public override DataTypes.Object Calculate(DataTypes.Object firstParameter, DataTypes.Object secondParameter)
        => (DataTypes.Object)Activator.CreateInstance(Type.GetUnderlyingType(ReturnType),
                                                      args: new object?[] { firstParameter.Value < secondParameter.Value, Token })!;
}
