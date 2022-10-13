using FAIL.ElementTree;
using Microsoft.Extensions.DependencyInjection;

namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class IdentifierParser : IParserComponent
{
    private readonly TokenReader Reader;
    private readonly IServiceProvider ServiceProvider;


    public IdentifierParser(TokenReader reader, IServiceProvider serviceProvider)
    {
        Reader = reader;
        ServiceProvider = serviceProvider;
    }


    public AST Parse(Scope scope, Token? token)
    {
        _ = Reader.ConsumeCurrentToken();

        return Reader.CurrentToken!.Value.Type switch
        {
            TokenType.Assignment => ServiceProvider.GetRequiredService<AssignmentParser>().Parse(scope, token!.Value), // test = 42;
            TokenType.SelfAssignment => ServiceProvider.GetRequiredService<SelfAssignmentParser>().Parse(scope, token!.Value), // test += 42;
            TokenType.OpeningParenthese => ServiceProvider.GetRequiredService<FunctionCallParser>().Parse(scope, token!.Value), // Test();
            TokenType.IncrementalOperator => ServiceProvider.GetRequiredService<IncrementalOperatorParser>().Parse(scope, token!.Value), // test++;
            TokenType.Accessor => ServiceProvider.GetRequiredService<TypeMemberParser>().Parse(scope, token!.Value), // test.ToString();
            _ => new Reference(scope.GetValidVariable(token!.Value.Value, token!.Value), scope, token)
        };
    }
}
