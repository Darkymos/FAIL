using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
public class Type : AST
{
	public string Name { get; set; }


	public Type(string name, Token? token = null) : base(token) => Name = name[0].ToString().ToUpper() + name[1..];

	public override Instance? Call() => throw new NotImplementedException();
	public override Type GetType() => this;
	public override string ToString() => Name;

	public static bool operator ==(Type first, Type second) => first.Name == second.Name;
	public static bool operator !=(Type first, Type second) => first.Name != second.Name;
	public override bool Equals(object? obj) => obj is not null && Name == ((Type)obj).Name;
	public override int GetHashCode() => Name.GetHashCode();

	public Object CreateInstance(dynamic value, Token? token) => Name switch
	{
		"String" => new BuiltIn.DataTypes.String(value, token),
		"Boolean" => new BuiltIn.DataTypes.Boolean(value, token),
		"Char" => new BuiltIn.DataTypes.Char(value, token),
		"Double" => new BuiltIn.DataTypes.Double(value, token),
		"Integer" => new BuiltIn.DataTypes.Integer(value, token),
		_ => throw new NotImplementedException()
	};

	public static System.Type GetType(Type type) => type.Name switch
	{
		"String" => typeof(BuiltIn.DataTypes.String),
		"Boolean" => typeof(BuiltIn.DataTypes.Boolean),
		"Char" => typeof(BuiltIn.DataTypes.Char),
		"Double" => typeof(BuiltIn.DataTypes.Double),
		"Integer" => typeof(BuiltIn.DataTypes.Integer),
		_ => throw new NotImplementedException()
	};
}
