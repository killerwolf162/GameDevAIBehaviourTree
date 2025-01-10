public class BTAlwaysTrue : BTDecorator
{
    public BTAlwaysTrue(BTBaseNode child) : base(child)
    {
    }

    protected override TaskStatus OnUpdate()
    {
        var result = child.Tick();
        if (result != TaskStatus.Running)
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Running;
    }
}
