using FAIL.Language_Integration;

namespace FAIL.Element_Tree;
internal class Function : AST
{
    public string Name { get; }
    public string ReturnType { get; }
    public CommandList? Body { get; }


    public Function(string name, string returnType, CommandList? body = null, Token? token = null) : base(token)
    {
        Name = name;
        ReturnType = returnType;
        Body = body;
    }


    public override dynamic? Call() => Body!.Call();
}
