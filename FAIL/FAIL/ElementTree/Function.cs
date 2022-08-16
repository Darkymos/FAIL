using FAIL.Exceptions;
using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;

internal record FunctionOverload(Type ReturnType, CommandList Parameters, CommandList Body);
internal class Function : AST
{
    public string Name { get; }
    public List<FunctionOverload> Overloads { get; } = new();
    public FunctionOverload? Current { get; private set; }


    public Function(string name, Token? token = null) : base(token) => Name = name;


    public void AddOverload(FunctionOverload overload)
    {
        ValidateOverload(overload);
        Overloads.Add(overload);
        Current ??= Overloads.First();
    }
    private void ValidateOverload(FunctionOverload overload)
    {
        foreach (var functionOverload in Overloads.Where(x => x.Parameters.Commands.Entries.Count == overload.Parameters.Commands.Entries.Count))
        {
            for (var i = 0; i < functionOverload.Parameters.Commands.Entries.Count; i++)
                if (functionOverload.Parameters.Commands.Entries[i].GetType() != overload.Parameters.Commands.Entries[i].GetType()) return;

            throw ExceptionCreator.OverloadAlreadyExists(Name);
        }
    }
    public void SetCurrentOverload(FunctionOverload overload) => Current = Overloads[Overloads.IndexOf(overload)];
    public bool HasOverload(CommandList parameters) 
        => Overloads.Any(x => ValidateParameters(x.Parameters.Commands.Entries, parameters.Commands.Entries));
    public FunctionOverload? GetOverload(CommandList parameters) 
        => Overloads.FirstOrDefault(x => ValidateParameters(x.Parameters.Commands.Entries, parameters.Commands.Entries));

    private static bool ValidateParameters(List<AST> expected, List<AST> given)
    {
        if (given.Count != expected.Count && NonOptionalParametersMissing(expected, given)) return false;

        var isValid = true;

        for (var i = 0; i < given.Count; i++)
        {
            if (!Parser.CheckType(given[i].GetType(), expected[i].GetType()))
                isValid = false;
        }

        return isValid;
    }
    private static bool NonOptionalParametersMissing(List<AST> expected, List<AST> given)
    {
        for (var i = 0; i < expected.Count; i++)
        {
            if (expected[i] is Variable var && !var.IsSet() && given.Count <= i) return true;
        }

        return false;
    }

    public override DataTypes.Object? Call()
    {
        try
        {
            _ = (Current?.Body.Call());
            return null;
        }
        catch (ReturnException ex)
        {
            return ex.Value;
        }
    }
    public override Type GetType() => Current is null ? new Type("Undefined") : Current!.ReturnType;
}
