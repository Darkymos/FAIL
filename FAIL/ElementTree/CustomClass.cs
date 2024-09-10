using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class CustomClass : Object
{
    public CommandList Members { get; }


    public CustomClass(string name, CommandList members, Token? token = null) : base(name, token) => Members = members;


    public CustomClass CreateInstance(CommandList parameters) => this;
}
