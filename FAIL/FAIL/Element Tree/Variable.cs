using FAIL.Language_Integration;

namespace FAIL.Element_Tree;
internal class Variable : AST
{
    public string Name { get; init; }
    private AST Value { get; set; }


    public Variable(string name, AST value, Token? token = null) : base(token)
    {
        Name = name;
        Value = value;
    }


    public override dynamic? Call() => Value.Call();

    public void Reassign(AST value) => Value = value;
}
