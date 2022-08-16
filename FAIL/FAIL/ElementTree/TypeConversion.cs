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
        try
        {
            var generic = typeof(TypeConversion).GetMethod("ConvertTo")!.MakeGenericMethod(Type.GetUnderlyingType(NewType));
            return generic.Invoke(null, new object[] { Value.Call()! }) is DataTypes.Object result
                ? result
                : throw new InvalidCastException();
        }
        catch (InvalidCastException)
        {
            var type = Type.GetUnderlyingType(Value.GetType());
            var conversionOperator = type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                                         .Where(m => m.Name == "op_Explicit")
                                         .Where(m => m.ReturnType == Type.GetUnderlyingType(NewType))
                                         .FirstOrDefault(m => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == type);

            return conversionOperator!.Invoke(null, new object[] { Value.Call()! })! as DataTypes.Object;
        }
    }
    public override Type GetType() => NewType;

    public static T? ConvertTo<T>(DataTypes.Object @object) where T : DataTypes.Object => @object as T;
}
