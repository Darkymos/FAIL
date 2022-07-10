namespace FAIL.Element_Tree;
internal class Scope
{
    public List<AST> Entries { get; }
    public Scope[] SharedScopes { get; }


    public Scope(List<AST> entries, params Scope[] sharedScopes)
    {
        Entries = entries;
        SharedScopes = sharedScopes;
    }


    public AST? Search(Func<AST, bool> predicate)
    {
        var entry = Entries.Where(predicate).FirstOrDefault();
        if (entry is null)
        {
            foreach (var scope in SharedScopes)
            {
                entry = scope.Search(predicate);
                if (entry is not null) return entry;
            }
        }

        return entry;
    }
    public void Add(AST item) => Entries.Add(item);
}
