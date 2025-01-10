public class BTGenericAction<T> : BTBaseNode
{
    protected System.Action<T> action;
    protected T target;

    public BTGenericAction(System.Action<T> action, T target)
    {
        this.action = action;
        this.target = target;
    }

    protected override TaskStatus OnUpdate()
    {
        action?.Invoke(target);
        return TaskStatus.Success;
    }
}