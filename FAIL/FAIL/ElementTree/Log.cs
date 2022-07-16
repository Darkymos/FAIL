using FAIL.LanguageIntegration;
using static System.FormattableString;

namespace FAIL.ElementTree;
internal class Log : AST
{
    public AST? Command { get; }


    public Log(AST? command, Token? token = null) : base(token) => Command = command;


    public override dynamic? Call()
    {
        var result = Command?.Call();

        Interpreter.Logger!.Log(result, LogLevel.Debug);
        Console.WriteLine(Invariant($"{result}"));

        return result;
    }
}
