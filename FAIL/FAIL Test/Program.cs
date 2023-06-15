using FAIL;
using FAIL.LanguageIntegration;
using Microsoft.Extensions.Logging;

var ast = new CompilerHostBuilder()
.ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Trace))
.AddTokenizer<Tokenizer>()
.AddParser<Parser>()
.AddFile(@"C:\Users\Micha\Desktop\Darkymos\FAIL\FAIL\FAIL Test\Test.fail")
.Build()
.GetRequiredService<IParser>().Parse().Call();