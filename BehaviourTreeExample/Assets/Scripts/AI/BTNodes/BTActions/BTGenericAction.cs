public class BTGenericAction : BTBaseNode
{
    protected System.Action action;

    public BTGenericAction(System.Action action)
    {
        this.action = action;
    }

    protected override TaskStatus OnUpdate()
    {
        action?.Invoke();
        return TaskStatus.Success;
    }
}
