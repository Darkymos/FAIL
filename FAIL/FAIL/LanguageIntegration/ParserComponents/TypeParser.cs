using FAIL.ElementTree;

namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class TypeParser : IParserComponent
{
    private readonly TokenReader Reader;
    private readonly CommandParser CommandParser;
    private readonly CommandListParser CommandListParser;


    public TypeParser(TokenReader reader, CommandParser commandParser, CommandListParser commandListParser)
    {
        Reader = reader;
        CommandParser = commandParser;
        CommandListParser = commandListParser;
    }


    public AST Parse(Scope scope, out bool isBlock)
    {
        var type = Reader.CurrentToken!.Value;
        var identifier = Reader.ConsumeCurrentToken()!.Value;
        _ = Reader.ConsumeCurrentToken(TokenType.Identifier);

        return Reader.IsTypeOf(TokenType.OpeningParenthese)
            ? ParseFunction(scope, out isBlock, type, identifier)
            : ParseVar(scope, out isBlock, type, identifier);
    }

    private AST ParseVar(Scope scope, out bool isBlock, Token type, Token identifier)
    {
        isBlock = false;

        // identifier must be unique in scope (variables AND functions), local are superior to shared ones (identifier doesn't need to be unique)
        if (!scope.IsIdentifierUnique(identifier.Value)) throw ExceptionCreator.AlreadyDeclaredInScope(identifier);

        // unassigned variable (used in function parameters)
        if (!Reader.IsTypeOf(TokenType.Assignment)) return new Variable(identifier.Value, new ElementTree.Type(type.Value), null, token: identifier);

        // already assigned variable (get the value)
        _ = Reader.ConsumeCurrentToken(TokenType.Assignment);
        return new Variable(identifier.Value,
                            CommandParser.Parse(scope),
                            identifier);
        
        // TODO check type
    }
    private AST ParseFunction(Scope scope, out bool isBlock, Token type, Token identifier)
    {
        isBlock = true;

        // functions must declare specific types for their return value to avoid major issues with result types on calculations
        if (type.Type.ToString() == "var") throw ExceptionCreator.SpecificTypeNeeded(identifier.Value, identifier);

        // 'parameters' may be empty
        _ = Reader.ConsumeCurrentToken(TokenType.OpeningParenthese);
        var parameters = CommandListParser.Parse(TokenType.Separator, TokenType.ClosingParenthese);

        // functions must declare specific types for their parameters to avoid major issues with result types on calculations
        foreach (var parameter in parameters.Commands.Entries.Cast<Variable>().Where(parameter => parameter.Type.Name == "var"))
            throw ExceptionCreator.SpecificTypeNeeded(identifier.Value, parameter.Token!.Value);

        var body = CommandListParser.Parse(parameters.Commands, scope);

        // if there is a return type declared in front of the identifier, there has to be a return a the end (currently)
        if (type.Type != TokenType.Void && body.Commands.Entries.Last() is not Return)
            throw ExceptionCreator.FunctionMustReturnValue(identifier.Value);

        // if there is an return type, we have to check it, funtions without return types may return something, which just won't be validated
        // TODO check type

        var existingFunction = scope.GetFunctionFromScope(identifier.Value) as Function;
        if (existingFunction is not null)
        {
            existingFunction.AddOverload(new(new ElementTree.Type(type.Value), parameters, body));
            return existingFunction;
        }

        // create the function boilerplate WITHOUT any overload, then add it
        var function = new Function(identifier.Value, identifier);
        function.AddOverload(new(new ElementTree.Type(type.Value), parameters, body));

        return function;
    }
}
