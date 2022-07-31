using FAIL.LanguageIntegration;
using Microsoft.CSharp.RuntimeBinder;

namespace FAIL.ElementTree.BinaryOperators;
internal class Equal : BinaryOperator
{
    public Equal(AST? firstParameter, AST? secondParameter, Token? token = null) : base(firstParameter, secondParameter, token)
    {
    }


    public override DataTypes.Object Calculate(DataTypes.Object firstParameter, DataTypes.Object secondParameter) 
        => new DataTypes.Boolean(firstParameter.Value == secondParameter.Value);
}
