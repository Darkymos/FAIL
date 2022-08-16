using FAIL.LanguageIntegration;

namespace FAIL.ElementTree.BinaryOperators;
internal class Addition : BinaryOperator
{
    public Addition(AST firstParameter, AST secondParameter, Token? token = null) : base(firstParameter, secondParameter, token)
    {
    }


    public override DataTypes.Object Calculate(DataTypes.Object firstParameter, DataTypes.Object secondParameter) 
        => (DataTypes.Object)Activator.CreateInstance(Type.GetUnderlyingType(GetCombinedType()), firstParameter.Value + secondParameter.Value, Token)!;
}
