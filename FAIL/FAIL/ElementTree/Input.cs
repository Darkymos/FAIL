using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class Input : AST
{
    public AST Command { get; }


    public Input(AST command, Token? token = null) : base(token) => Command = command;

    public override dynamic? Call()
    {
        var result = Command.Call();

        Console.Write(result);
        return Console.ReadLine();
    }
}
