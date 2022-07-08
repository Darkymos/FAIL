using FAIL.Language_Integration;
using static System.FormattableString;

namespace FAIL.Element_Tree;
internal class Log : AST
{
    private AST? Command { get; init; }


    public Log(AST? command, Token? token = null) : base(token) => Command = command;


    public override dynamic? Call()
    {
        var result = Command?.Call();

        Interpreter.Logger!.Log(result, LogLevel.Debug);
        Console.WriteLine(Invariant($"{result}"));

        return result;
    }
}
