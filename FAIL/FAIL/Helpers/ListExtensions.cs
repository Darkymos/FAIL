using OneOf;
using OneOf.Types;

namespace FAIL.Helpers;
public static class ListExtensions
{
    public static List<T> AddWhile<T>(this List<T> collection, Func<bool> predicate, Func<List<T>, T> elementReceiver)
    {
        while (predicate()) collection.Add(elementReceiver(collection));
        return collection;
    }
    public static List<T> AddOptionalWhile<T>(this List<T> collection, Func<bool> predicate, Func<List<T>, OneOf<T, None>> elementReceiver)
    {
        while (predicate()) if (elementReceiver(collection) is T element) collection.Add(element);
        return collection;
    }

    public static List<T> Expect<T>(this List<T> collection, Func<List<T>, bool> requirement, Action<List<T>>? success = null, Action<List<T>>? failure = null)
    {
        if (requirement(collection)) success?.Invoke(collection);
        else failure?.Invoke(collection);
        return collection;
    }

    public static List<T> ExecuteIf<T>(this List<T> collection, Func<List<T>, bool> predicate, Action<List<T>> success, Action<List<T>>? failure = null)
    {
        if (predicate(collection)) success?.Invoke(collection);
        else failure?.Invoke(collection);
        return collection;
    }
    public static TOut ExecuteIfAndGet<T, TOut>(this List<T> collection, Func<List<T>, bool> predicate, Func<List<T>, TOut> success, Func<List<T>, TOut> failure)
        => predicate(collection) ? success.Invoke(collection)! : failure.Invoke(collection)!;

    public static List<T> Consume<T>(this List<T> elements)
    {
        elements.RemoveAt(0);
        return elements;
    }
    public static List<T> ConsumeExpected<T>(this List<T> elements, Func<T, bool> predicate, Action<T>? failure = null)
    {
        if (!predicate(elements.First())) failure?.Invoke(elements.First());

        elements.RemoveAt(0);
        return elements;
    }

    public static T ConsumeAndGet<T>(this List<T> elements)
    {
        var element = elements.First();
        elements.RemoveAt(0);
        return element;
    }
    public static T ConsumeAndGetExpected<T>(this List<T> elements, Func<T, bool> predicate, Action<T>? failure = null)
    {
        var element = elements.First();

        if (!predicate(elements.First())) failure?.Invoke(elements.First());

        elements.RemoveAt(0);
        return element;
    }

    public static T GetCurrent<T>(this List<T> elements) => elements.First();
    public static List<T> GetCurrent<T>(this List<T> elements, out T element)
    {
        element = elements.First();
        return elements;
    }
}
