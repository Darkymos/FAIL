using FAIL.ElementTree;

namespace FAIL.Helpers;
public static class ScopeExtensions
{
	public static Scope AddWhile(this Scope collection, Func<bool> predicate, Func<Scope, AST> elementReceiver)
	{
		while (predicate()) collection.Add(elementReceiver(collection));
		return collection;
	}

	public static Scope Expect(this Scope collection, Func<Scope, bool> requirement, Action<Scope>? success = null, Action<Scope>? failure = null)
	{
		if (requirement(collection)) success?.Invoke(collection);
		else failure?.Invoke(collection);
		return collection;
	}

	public static Scope ExecuteIf(this Scope collection, Func<Scope, bool> predicate, Action<Scope> success)
	{
		if (predicate(collection)) success?.Invoke(collection);
		return collection;
	}
}
