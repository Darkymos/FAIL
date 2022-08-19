using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class Type : AST
{
    public string Name { get; set; }


    public Type(string name, Token? token = null) : base(token) => Name = name[0].ToString().ToUpper() + name[1..];

    public override DataTypes.Object? Call() => throw new NotImplementedException();
    public override Type GetType() => this;
    public override string ToString() => Name;

    public static bool operator ==(Type first, Type second) => first.Name == second.Name;
    public static bool operator !=(Type first, Type second) => first.Name != second.Name;
    public override bool Equals(object? obj) => obj is not null && Name == ((Type)obj).Name;
    public override int GetHashCode() => Name.GetHashCode();

    public static System.Type GetUnderlyingType(Type type) => System.Type.GetType($"FAIL.ElementTree.DataTypes.{type.Name}")!;
}
