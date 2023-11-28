using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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