using FAIL.LanguageIntegration;

namespace FAIL.ElementTree.BinaryOperators;
internal class Addition : BinaryOperator
{
    public Addition(AST? firstParameter, AST? secondParameter, Token? token = null) : base(firstParameter, secondParameter, token)
    {
    }


    public override DataTypes.Object Calculate(DataTypes.Object firstParameter, DataTypes.Object secondParameter)
    {
        try
        {
            return (DataTypes.Object)Activator.CreateInstance(Type.GetUnderlyingType(GetCombinedType()), firstParameter.Value + secondParameter.Value, Token)!;
        }
        catch (InvalidOperationException)
        {
            return new DataTypes.String(firstParameter.Value.ToString() + secondParameter.Value.ToString());
        }
    }
}
