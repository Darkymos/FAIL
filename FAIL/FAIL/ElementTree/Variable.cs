using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class Variable : AST
{
    public string Name { get; }
    public Type Type { get; }
    private AST? Value { get; set; }


    public Variable(string name, Type type, AST? value = null, Token? token = null) : base(token)
    {
        Name = name;
        Value = value;
    }


    public override dynamic? Call() => Value?.Call();

    public void Reassign(AST value) => Value = value;
    public bool IsSet() => Value is not null;
}
