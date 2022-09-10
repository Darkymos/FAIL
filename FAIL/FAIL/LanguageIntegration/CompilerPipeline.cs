using FAIL.ElementTree;

namespace FAIL.LanguageIntegration;

internal class CompilerPipeline
{
    private readonly ITokenizer Tokenizer;
    private readonly IParser Parser;
    private readonly List<ICompilerComponent> Components;

    private List<Token>? TokenBuffer;
    private AST? Buffer;


    public CompilerPipeline(ITokenizer tokenizer, IParser parser)
    {
        Tokenizer = tokenizer;
        Parser = parser;
        Components = new();
    }

    public Tout? AddComponent<Tout>() where Tout : class, ICompilerComponent, new()
    {
        if (Components.Any(x => x is Tout)) return null;

        var component = new Tout();

        Components.Add(component);
        return component;
    }
    public Tout? GetComponent<Tout>() where Tout : class, ICompilerComponent
    {
        try
        {
            return (Tout)Components.First(x => x is Tout);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public Instance? Call(string code, string fileName)
    {
        TokenBuffer = Tokenizer.Call(code, fileName);
        Buffer = Parser.Call(TokenBuffer);

        foreach (var component in Components) Buffer = component.Call(Buffer);

        return Buffer!.Call();
    }
}