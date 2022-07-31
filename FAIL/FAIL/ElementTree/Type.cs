using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class Type : AST
{
    public string Name { get; set; }


    public Type(string name, Token? token = null) : base(token) => Name = name;



    public override DataTypes.Object? Call() => throw new NotImplementedException();
    public override Type GetType() => this;
    public override string ToString() => Name;

    public static bool operator ==(Type first, Type second) => first.Name == second.Name;
    public static bool operator !=(Type first, Type second) => first.Name != second.Name;

    public static System.Type GetUnderlyingType(Type type) => System.Type.GetType($"FAIL.ElementTree.DataTypes.{type.Name}")!;
}
