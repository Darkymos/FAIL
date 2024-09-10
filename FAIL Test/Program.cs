using FAIL;
using FAIL.LanguageIntegration;
using Microsoft.Extensions.Logging;

var ast = new CompilerHostBuilder()
.ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Trace))
.AddTokenizer<Tokenizer>()
.AddParser<Parser>()
.AddFile(Path.Combine(Directory.GetCurrentDirectory().Split(@"\bin")[0], "Test.fail"))
.Build()
.GetRequiredService<IParser>().Parse().Call();