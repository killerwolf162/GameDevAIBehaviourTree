public class BTGenericAction : BTBaseNode
{
    protected System.Action action;

    public BTGenericAction(System.Action _action)
    {
        this.action = _action;
    }

    protected override TaskStatus OnUpdate()
    {
        action?.Invoke();
        return TaskStatus.Success;
    }
}
