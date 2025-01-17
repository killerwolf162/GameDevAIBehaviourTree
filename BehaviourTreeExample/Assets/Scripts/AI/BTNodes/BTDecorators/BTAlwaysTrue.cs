public class BTAlwaysTrue : BTDecorator
{
    public BTAlwaysTrue(BTBaseNode _child) : base(_child)
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
