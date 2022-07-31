using FAIL.LanguageIntegration;
using System.Reflection;

namespace FAIL.ElementTree;
internal class TypeConversion : AST
{
    public AST Value { get; }
    public Type NewType { get; }

    public TypeConversion(AST value, Type newType, Token? token = null) : base(token)
    {
        Value = value;
        NewType = newType;
    }

    public override DataTypes.Object? Call()
    {
        var type = Type.GetUnderlyingType(Value.GetType());

        var conversionOperator = type.GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Where(m => m.Name == "op_Explicit")
            .Where(m => m.ReturnType == Type.GetUnderlyingType(NewType))
            .Where(m => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == type)
            .FirstOrDefault();

        return conversionOperator!.Invoke(null, new object[] { Value.Call()! })! as DataTypes.Object;
    }
    public override Type GetType() => NewType;
}
