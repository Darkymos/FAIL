using FAIL.LanguageIntegration;

namespace FAIL.ElementTree;
internal class Reference : AST
{
    public Variable Variable { get; }


    public Reference(Variable variable, Scope scope, Token? token = null) : base(token) => Variable = variable;//CheckForAssignedVariable(scope);

    public override Instance? Call() => Variable.IsSet() ? Variable.Call() : throw ExceptionCreator.UseOfUnassignedVariable(Variable.Name, Token);

    public override Type GetType() => Variable.GetType();

    //private void CheckForAssignedVariable(Scope scope)
    //{
    //    if (!Variable.IsSet() && scope.Search(x => x is Assignment assignment && assignment.AssignTo.Name == Variable.Name) is null)
    //        throw ExceptionCreator.UseOfUnassignedVariable(Variable.Name, Token);
    //}
}
