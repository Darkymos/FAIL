using FAIL.Metadata;
using static FAIL.BuiltIn.BuiltInFunctions;

namespace FAIL.ElementTree;
internal class BuiltInFunctionCall : AST
{
    public string Name { get; }
    public CommandList Parameters { get; }


    public BuiltInFunctionCall(string name, CommandList parameters)
    {
        Name = name;
        Parameters = parameters;

        _ = ValidateParameters(Functions[Name].Parameters, parameters.Commands.Entries);
    }

    private static bool ValidateParameters(List<ParameterInfo> expected, List<AST> given)
    {
        if (given.Count != expected.Count && NonOptionalParametersMissing(expected, given)) return false;

        // TODO check type
        return true;
    }
    private static bool NonOptionalParametersMissing(List<ParameterInfo> expected, List<AST> given)
    {
        for (var i = 0; i < expected.Count; i++) if (!expected[i].Optional && given.Count <= i) return true;
        return false;
    }

    public override Instance? Call() => Functions[Name].Function.Invoke(Parameters);
    public override Type GetType() => Functions[Name].ReturnType;
}
