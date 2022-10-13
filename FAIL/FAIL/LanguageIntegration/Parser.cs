using FAIL.ElementTree;
using FAIL.Helpers;
using FAIL.LanguageIntegration.ParserComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FAIL.LanguageIntegration;
internal sealed class Parser : IParser
{
    private IHost? Host;

    public CommandList Call(List<Token> tokens)
    {
        var reader = new TokenReader(tokens);

        Host = Microsoft.Extensions.Hosting.Host
        .CreateDefaultBuilder()
        .ConfigureServices((services) =>
        {
            _ = services.AddSingleton(reader);

            _ = services.AddActivatorServices<IParserComponent, ParserComponentActivator>();
        })
        .Build();

        Host.Services.GetRequiredService<ParserComponentActivator>().ActivateAsync().Wait();

        if (reader.IsEOT()) return new(); // empty file

        var topLevelStatements = Host.Services.GetRequiredService<CommandListParser>().Parse(TokenType.EndOfStatement);

        // is there a character, that shouldn't be there (a non finished command)?
        return reader.IsEOT() ? topLevelStatements : throw ExceptionCreator.UnexpectedToken(reader.CurrentToken!.Value);
    }
}
