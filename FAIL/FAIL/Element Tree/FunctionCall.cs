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

    private void ValidateParameters()
    {
        var expected = Function.Parameters.Commands.Entries;
        var given = Parameters.Commands.Entries;

        if (given.Count != expected.Count)
            throw ExceptionCreator.WrongParameterCount(expected.Count, given.Count, Function.Name);

        // check for datatype
        /*for (var i = 0; i < expected.Count; i++)
        {
            if (given[i].Call())
        }*/
    }
    private void MapParameters()
    {
        var assignTo = Function.Parameters.Commands.Entries;
        var values = Parameters.Commands.Entries;

        for (var i = 0; i < assignTo.Count; i++)
        {
            (assignTo[i] as Variable)!.Reassign(values[i]);
        }
    }
}
