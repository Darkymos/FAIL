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

        ValidateParameters();
    }


    public override dynamic? Call()
    {
        MapParameters();

        return Function.Call();
    }
    public override Type GetType() => Function.GetType();

    private void ValidateParameters()
    {
        var expected = Function.Parameters.Commands.Entries;
        var given = Parameters.Commands.Entries;

        if (given.Count != expected.Count && NonOptionalParametersMissing(expected, given))
            throw ExceptionCreator.WrongParameterCount(expected.Count, given.Count, Function.Name, Token?.Value);

        for (var i = 0; i < given.Count; i++)
            Parser.CheckType(given[i].GetType(), expected[i].GetType(), (expected[i] as Variable)!.Name, given[i].Token!.Value);
    }
    private void MapParameters()
    {
        var assignTo = Function.Parameters.Commands.Entries;
        var values = Parameters.Commands.Entries;

        for (var i = 0; i < values.Count; i++)
        {
            (assignTo[i] as Variable)!.Reassign(values[i]);
        }
    }
    private bool NonOptionalParametersMissing(List<AST> expected, List<AST> given)
    {
        for (var i = 0; i < expected.Count; i++)
        {
            if (expected[i] is Variable var && !var.IsSet() && given.Count <= i) return true;
        }

        return false;
    }
}
