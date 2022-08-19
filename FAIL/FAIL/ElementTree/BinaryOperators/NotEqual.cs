using FAIL.LanguageIntegration;
using FAIL.Metadata;
using Microsoft.CSharp.RuntimeBinder;

namespace FAIL.ElementTree.BinaryOperators;
internal class NotEqual : BinaryOperator
{
    public NotEqual(AST firstParameter, AST secondParameter, Token? token = null) : base(firstParameter, secondParameter, token)
        => ReturnType = GetReturnType(BinaryOperation.NotEqual);

    public override DataTypes.Object Calculate(DataTypes.Object firstParameter, DataTypes.Object secondParameter)
        => (DataTypes.Object)Activator.CreateInstance(Type.GetUnderlyingType(ReturnType),
                                                      args: new object?[] { firstParameter.Value != secondParameter.Value, Token })!;
}
