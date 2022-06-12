using FAIL.Language_Integration;

namespace FAIL.ElementTree;
internal class CommandList : AST
{
    public List<AST?> Commands { get; }

    public CommandList(List<AST?> commands, Token? token = null) : base(token) => Commands = commands;

    public override object? Call()
    {
        object? lastResult = null;

        foreach (var cmd in Commands)
        {
            lastResult = cmd is not null ? cmd.Call() : null;
        }

        return lastResult;
    }
}
