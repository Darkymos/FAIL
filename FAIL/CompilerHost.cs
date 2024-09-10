using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FAIL;
public sealed class CompilerHost
{
	public IHost Host { get; }


	public CompilerHost(IHost host)
	{
		Host = host;

		_ = Host.RunAsync();
	}


	public T GetRequiredService<T>() where T : notnull => Host.Services.GetRequiredService<T>();
	public T? GetService<T>() where T : notnull => Host.Services.GetService<T>();
}
