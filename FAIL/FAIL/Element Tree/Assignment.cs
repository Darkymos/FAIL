using FAIL.Language_Integration;

namespace FAIL.Element_Tree;
internal class Assignment : AST
{
    public Variable AssignTo { get; init; }
    public AST Value { get; init; }


    public Assignment(Variable assignTo, AST value, Token? token = null) : base(token)
    {
        AssignTo = assignTo;
        Value = value;
    }


    public override dynamic? Call()
    {
        AssignTo.Reassign(Value);
        return null;
    }
}
