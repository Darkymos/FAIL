using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class FunctionCall : AST
{
    public Function Function { get; }
    public CommandList Parameters { get; }


    public FunctionCall(Function function, CommandList parameters, Token? token = null) : base(token)
    {
        Function = function;
        Parameters = parameters;

        if (!Function.HasOverload(Parameters)) throw ExceptionCreator.OverloadNotFound(Function.Name);
    }


    public override DataTypes.Object? Call()
    {
        Function.SetCurrentOverload(Function.GetOverload(Parameters)!);
        MapParameters();

        return Function.Call();
    }
    public override Type GetType() => Function.GetType();

    private void MapParameters()
    {
        var assignTo = Function.Current!.Parameters.Commands.Entries;
        var values = Parameters.Commands.Entries;

        for (var i = 0; i < values.Count; i++) (assignTo[i] as Variable)!.Reassign(values[i]);
    }
}
