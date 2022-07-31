using FAIL.LanguageIntegration;

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

    public override DataTypes.Object Call()
    {
        var type = System.Type.GetType($"FAIL.ElementTree.DataTypes.{NewType.Name}")!;
        var method = typeof(DataTypes.Object).GetMethod("ConvertTo");
        var genericMethod = method!.MakeGenericMethod(type);
        return (genericMethod.Invoke(Value.Call(), null)! as DataTypes.Object)!;
    }

    public override Type GetType() => NewType;
}
