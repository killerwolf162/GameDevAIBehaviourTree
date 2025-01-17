public class BTGenericAction<T> : BTBaseNode
{
    protected System.Action<T> action;
    protected T target;

    public BTGenericAction(System.Action<T> _action, T _target)
    {
        this.action = _action;
        this.target = _target;
    }

    protected override TaskStatus OnUpdate()
    {
        action?.Invoke(target);
        return TaskStatus.Success;
    }
}