using FAIL.Language_Integration;

namespace FAIL.Element_Tree;
internal class Log : AST
{
    private AST? Command { get; init; }


    public Log(AST? command, Token? token = null) : base(token) => Command = command;


    public override dynamic? Call()
    {
        var result = Command?.Call();

        Interpreter.Logger!.Log(result, Command, LogLevel.Info);
        Console.WriteLine(result);

        return result;
    }
}
