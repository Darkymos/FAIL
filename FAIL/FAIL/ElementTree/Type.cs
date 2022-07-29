using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class Type : AST
{
    public string Name { get; set; }


    public Type(string name, Token? token = null) : base(token) => Name = name;



    public override dynamic? Call() => throw new NotImplementedException();
}
