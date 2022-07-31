using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class Input : AST
{
    public AST Command { get; }


    public Input(AST command, Token? token = null) : base(token) => Command = command;

    public override DataTypes.Object? Call()
    {
        var result = Command?.Call();

        if (result is not null) Console.Write(result);
        return new DataTypes.String(Console.ReadLine()!);
    }
    public override Type GetType() => Command.GetType();
}
