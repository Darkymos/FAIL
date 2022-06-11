using FAIL.ElementTree;
using FAIL.Language_Integration;

var parser = new Parser(@"C:\Users\Micha\Desktop\Darkymos\Scripts\FAIL Test\Test.fail");

foreach (var item in (parser.Parse() as CommandList).Commands)
{
    Console.WriteLine(item.Token.Value);
}