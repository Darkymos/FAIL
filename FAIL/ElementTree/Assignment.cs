using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class Assignment : AST
{
    public Variable AssignTo { get; init; }
    public AST Value { get; init; }


    public Assignment(Variable assignTo, AST value, Token? token = null) : base(token)
    {
        AssignTo = assignTo;
        Value = value;
    }


    public override Instance? Call()
    {
        AssignTo.Reassign(Value.Call()!);
        return AssignTo.Call();
    }
    public override Type GetType() => Value.GetType();
}
