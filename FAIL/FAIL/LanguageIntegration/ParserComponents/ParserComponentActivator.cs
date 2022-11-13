namespace FAIL.LanguageIntegration.ParserComponents;
internal sealed class ParserComponentActivator
{
    private readonly IEnumerable<IParserComponent> Services;


    public ParserComponentActivator(IEnumerable<IParserComponent> services) => Services = services;


    public async Task ActivateAsync() =>
        //foreach (var service in Services) await service.StartAsync();
        await Task.CompletedTask;
}
