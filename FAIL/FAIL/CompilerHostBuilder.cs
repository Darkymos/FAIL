using FAIL.LanguageIntegration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FAIL;
public sealed class CompilerHostBuilder
{
	private readonly HostApplicationBuilder Builder = Host.CreateApplicationBuilder();

	private readonly List<string> Files = new();


	public CompilerHostBuilder AddSingleton<T>() where T : class
	{
		_ = Builder.Services.AddSingleton<T>();
		return this;
	}
	public CompilerHostBuilder AddSingleton<T>(T service) where T : class
	{
		_ = Builder.Services.AddSingleton(service);
		return this;
	}
	public CompilerHostBuilder AddSingleton(Type type)
	{
		_ = Builder.Services.AddSingleton(type);
		return this;
	}
	public CompilerHostBuilder AddSingleton<T, TImp>() where T : class where TImp : class, T
	{
		_ = Builder.Services.AddSingleton<T, TImp>();
		return this;
	}
	public CompilerHostBuilder AddSingleton(Type type, Type implementation)
	{
		_ = Builder.Services.AddSingleton(type, implementation);
		return this;
	}
	public CompilerHostBuilder AddScoped<T>() where T : class
	{
		_ = Builder.Services.AddScoped<T>();
		return this;
	}
	public CompilerHostBuilder AddScoped(Type type)
	{
		_ = Builder.Services.AddScoped(type);
		return this;
	}
	public CompilerHostBuilder AddScoped<T, TImp>() where T : class where TImp : class, T
	{
		_ = Builder.Services.AddScoped<T, TImp>();
		return this;
	}
	public CompilerHostBuilder AddScoped(Type type, Type implementation)
	{
		_ = Builder.Services.AddScoped(type, implementation);
		return this;
	}
	public CompilerHostBuilder AddTransient<T>() where T : class
	{
		_ = Builder.Services.AddTransient<T>();
		return this;
	}
	public CompilerHostBuilder AddTransient(Type type)
	{
		_ = Builder.Services.AddTransient(type);
		return this;
	}
	public CompilerHostBuilder AddTransient<T, TImp>() where T : class where TImp : class, T
	{
		_ = Builder.Services.AddTransient<T, TImp>();
		return this;
	}
	public CompilerHostBuilder AddTransient(Type type, Type implementation)
	{
		_ = Builder.Services.AddTransient(type, implementation);
		return this;
	}

	public CompilerHostBuilder ConfigureLogging(Action<ILoggingBuilder> configuration)
	{
		configuration(Builder.Logging);
		return this;
	}

	public CompilerHostBuilder AddTokenizer<T>() where T : class, ITokenizer => AddSingleton<ITokenizer, T>();
	public CompilerHostBuilder AddParser<T>() where T : class, IParser => AddSingleton<IParser, T>();

	public CompilerHostBuilder AddFile(string fileName)
	{
		Files.Add(fileName);
		return this;
	}

	public CompilerHost Build()
	{
		_ = Builder.Services
			.AddSingleton(Files);

		var host = Builder.Build();

		return new CompilerHost(host);
	}
}