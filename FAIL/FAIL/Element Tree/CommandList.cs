using FAIL.Language_Integration;

namespace FAIL.Element_Tree;
internal class CommandList : AST
{
    public List<AST?> Commands { get; init; }        


    public CommandList(List<AST?> commands, Token? token = null) : base(token) => Commands = commands;


    public override dynamic? Call()
    {
        var results = ProcessAll();
        return results.Count == 0 ? null : results[^1];
    }
    public override string ToString() => $"{nameof(CommandList)} with {Commands.Count} elements";

    private List<dynamic?> ProcessAll()
    {
        var results = new List<dynamic?>();

        foreach (var command in Commands)
        {
            results.Add(command?.Call());
            Interpreter.Logger?.Log(results[^1], command, LogLevel.Debug);
        }

        return results;
    }
}
