using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class CommandList : AST
{
    public Scope Commands { get; }        


    public CommandList(Scope commands, Token? token = null) : base(token) => Commands = commands;


    public override DataTypes.Object? Call()
    {
        var results = ProcessAll();
        return results.Count == 0 ? null : results[^1];
    }
    public override string ToString() => $"{nameof(CommandList)} with {Commands.Entries.Count} elements";
    public override Type GetType() => new("Undefined");

    private List<dynamic?> ProcessAll()
    {
        var results = new List<dynamic?>();

        foreach (var command in Commands.Entries)
        {
            if (command as Variable is not null || command as Function is not null) continue;

            results.Add(command?.Call());
            Interpreter.Logger?.Log(results[^1], command, LogLevel.Info);
        }

        return results;
    }
}
