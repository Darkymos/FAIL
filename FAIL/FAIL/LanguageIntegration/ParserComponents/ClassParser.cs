using FAIL.ElementTree;

namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class ClassParser : IParserComponent
{
    private readonly TokenReader Reader;
    private readonly CommandListParser CommandListParser;


    public ClassParser(TokenReader reader, CommandListParser commandListParser)
    {
        Reader = reader;
        CommandListParser = commandListParser;
    }


    public AST Parse(Scope scope, out bool isBlock)
    {
        isBlock = true;

        var identifier = Reader.ConsumeCurrentToken(TokenType.Class);
        if (!scope.IsIdentifierUnique(identifier!.Value.Value)) throw ExceptionCreator.AlreadyDeclaredInScope(identifier!.Value);
        _ = Reader.ConsumeCurrentToken(TokenType.Identifier);

        var members = ParseClassBody(scope, identifier!.Value);

        return new CustomClass(identifier!.Value.Value, members, identifier);
    }

    private CommandList ParseClassBody(Scope scope, Token identifier)
    {
        _ = Reader.ConsumeCurrentToken(TokenType.OpeningBracket);

        var commands = new Scope(scope);

        //while (!Reader.IsTypeOf(TokenType.ClosingBracket)) commands.Add(ParseClassMember(commands));

        _ = Reader.ConsumeCurrentToken(TokenType.ClosingBracket);

        return new(commands);
    }
    //private AST ParseClassMember(Scope scope)
    //{
    //    if (Reader.IsTypeOf(TokenType.DataType))
    //    {
    //        var type = Reader.CurrentToken!.Value;
    //        var identifier = Reader.ConsumeCurrentToken();
    //        Reader.ConsumeCurrentToken();

    //        if (Reader.IsTypeOf(TokenType.OpeningParenthese)) return ParseMethod(scope, type, identifier);
    //    }
    //}
    private AST ParseConstructor(Scope scope, out bool isBlock)
    {
        isBlock = true;

        _ = Reader.ConsumeCurrentToken(TokenType.Init);
        var identifier = Reader.ConsumeCurrentToken(TokenType.Identifier);

        //if (scope.Search(x => x is ))

        // 'parameters' may be empty
        _ = Reader.ConsumeCurrentToken(TokenType.OpeningParenthese);
        var parameters = CommandListParser.Parse(TokenType.Separator, TokenType.ClosingParenthese);

        // functions must declare specific types for their parameters to avoid major issues with result types on calculations
        foreach (var parameter in parameters.Commands.Entries.Cast<Variable>().Where(parameter => parameter.Type.Name == "var"))
            throw ExceptionCreator.SpecificTypeNeeded("Constructor", parameter.Token!.Value);

        var body = CommandListParser.Parse(parameters.Commands, scope);

        var existingConstrcutor = scope.GetFunctionFromScope(identifier!.Value.Value) as Function;
        if (existingConstrcutor is not null)
        {
            //existingConstrcutor.AddOverload(new(new ElementTree.Type(type.Value), parameters, body));
            return existingConstrcutor;
        }

        // create the function boilerplate WITHOUT any overload, then add it
        var constructor = new Function(identifier!.Value.Value, identifier);
        //constructor.AddOverload(new(new ElementTree.Type(type.Value), parameters, body));

        return constructor;
    }
}
