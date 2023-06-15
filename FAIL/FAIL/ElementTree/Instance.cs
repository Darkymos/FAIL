using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
public class Instance : AST
{
	public Type Type { get; }
	public Object Value { get; }


	public Instance(Type type, dynamic value, Token? token = null) : base(token)
	{
		Type = type;
		Value = Type.CreateInstance(value, token);
	}
	public Instance(Type type, Scope scope, CommandList parameters, Token? token = null) : base(token)
	{
		Type = type;
		Value = (scope.Search(x => x is CustomClass @class && @class.Name == type.Name) as CustomClass)!.CreateInstance(parameters);
	}


	public override Instance? Call() => this;
	public override Type GetType() => Type;

	public Tout GetValueAs<Tout>() where Tout : Object => (Tout)Value;
}
