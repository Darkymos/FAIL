namespace FAIL.ElementTree;
internal class Scope
{
    public List<AST> Entries { get; }
    public Scope[] SharedScopes { get; }


    public Scope(params Scope[] sharedScopes) : this(new(), sharedScopes) { }
    public Scope(List<AST> entries, params Scope[] sharedScopes)
    {
        Entries = entries;
        SharedScopes = sharedScopes;
    }


    public AST? Search(Func<AST, bool> predicate, bool singleLayer = false)
    {
        var entry = Entries.FirstOrDefault(predicate);
        if (entry is not null) return entry;

        if (singleLayer) return null;

        foreach (var scope in SharedScopes) if (scope.Search(predicate) is (not null) and AST result) return result;

        return null;
    }
    public void Add(AST item) => Entries.Add(item);
}
