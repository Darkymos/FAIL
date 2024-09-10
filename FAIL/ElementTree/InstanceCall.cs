namespace FAIL.ElementTree;
internal class InstanceCall : AST
{
    public Reference Reference { get; }
    public AST NestedCommand { get; }


    public InstanceCall(Reference reference, AST nestedCommand)
    {
        Reference = reference;
        NestedCommand = nestedCommand;
    }


    public override Instance? Call() => NestedCommand.Call();
    public override Type GetType() => NestedCommand.GetType();
}
