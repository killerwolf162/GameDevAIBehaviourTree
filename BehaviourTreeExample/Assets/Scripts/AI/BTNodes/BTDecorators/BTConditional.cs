using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BTCondition : BTBaseNode
{
    protected Func<bool> condition;
    public BTCondition(System.Func<bool> _condition)
    {
        this.condition = _condition;
    }

    public virtual bool Evaluate() { return condition.Invoke(); }

    protected override TaskStatus OnUpdate()
    {
        return Evaluate() ? TaskStatus.Success : TaskStatus.Failed;
    }
}

public class BTCondition<T> : BTBaseNode
{
    protected Func<T, bool> condition;
    protected T target;
    public BTCondition(System.Func<T, bool> condition, T target)
    {
        this.condition = condition;
        this.target = target;
    }

    public virtual bool Evaluate() { return condition.Invoke(target); }

    protected override TaskStatus OnUpdate()
    {
        return Evaluate() ? TaskStatus.Success : TaskStatus.Failed;
    }
}
