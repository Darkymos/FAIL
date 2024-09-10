namespace FAIL.Helpers;
public static class GenericExtensions
{
    public static T Expect<T>(this T element, Func<T, bool> requirement, Action<T>? success = null, Action<T>? failure = null)
    {
        if (requirement(element)) success?.Invoke(element);
        else failure?.Invoke(element);
        return element;
    }

    public static T ExecuteIf<T>(this T element, Func<T, bool> predicate, Action<T> success, Action<T>? failure = null)
    {
        if (predicate(element)) success?.Invoke(element);
        else failure?.Invoke(element);
        return element;
    }
    public static TOut ExecuteIfAndGet<T, TOut>(this T element, Func<T, bool> predicate, Func<T, TOut> success, Func<T, TOut> failure)
        => predicate(element) ? success.Invoke(element)! : failure.Invoke(element)!;

    public static T Then<T>(this T element, Action<T> action)
    {
        action(element);
        return element;
    }
    public static TOut ThenTo<T, TOut>(this T element, Func<T, TOut> action) => action(element);

    public static T Store<T>(this T element, out T item)
    {
        item = element;
        return element;
    }
}
