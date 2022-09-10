using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class Variable : AST
{
    public string Name { get; }
    public Type Type { get; }
    private AST? Value { get; set; }


    public Variable(string name, AST value, Token? token = null) : base(token)
    {
        Name = name;
        Type = value.GetType();
        Value = value;
    }
    public Variable(string name, Type type, AST? value = null, Token? token = null) : base(token)
    {
        Name = name;
        Type = type;
        Value = value;
    }


    public override Instance? Call() => Value?.Call();
    public override Type GetType() => Type;

    public void Reassign(AST value) => Value = value;
    public bool IsSet() => Value is not null;
}
