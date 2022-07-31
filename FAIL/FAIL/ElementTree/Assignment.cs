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


    public override DataTypes.Object? Call()
    {
        AssignTo.Reassign(Value.Call()!);
        return null;
    }
    public override Type GetType() => Value.GetType();
}
