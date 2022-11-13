using FAIL.LanguageIntegration;

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

    public bool IsIdentifierUnique(string name)
    {
        return GetVariableFromScope(name, true) is null
&& GetFunctionFromScope(name, true) is null && GetClassFromScope(name, true) is null;
    }
    public bool IsDeclared(string name)
    {
        if (GetVariableFromScope(name) is not null) return true; // variable with the name found in scope
        return false; // not declared yet
    }
    public Variable? GetVariableFromScope(string name, bool singleLayer = false)
        => Search(x => x is Variable variable && variable.Name == name, singleLayer) as Variable;
    public Function? GetFunctionFromScope(string name, bool singleLayer = false)
        => Search(x => x is Function function && function.Name == name, singleLayer) as Function;
    public Object? GetClassFromScope(string name, bool singleLayer = false)
        => Search(x => x is Object @class && @class.Name == name, singleLayer) as ElementTree.Object;
    public Variable GetValidVariable(string name, Token token)
    {
        if (!IsDeclared(name)) throw ExceptionCreator.NotAssignedInScope(token); // their is currently no variable with this name

        var variable = GetVariableFromScope(name);
        return variable is null ? throw ExceptionCreator.VariableExpected() : variable;
    }
}
