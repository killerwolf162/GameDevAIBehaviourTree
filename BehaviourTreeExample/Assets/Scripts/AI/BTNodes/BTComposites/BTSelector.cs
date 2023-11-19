using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTSelector : BTComposite
{
    protected override TaskStatus OnUpdate()
    {
        for (var i = 0; i < children.Length; i++)
        {
            var result = children[i].Tick();
            switch (result)
            {
                case TaskStatus.Success: return TaskStatus.Success;
                case TaskStatus.Failed: continue;
                case TaskStatus.Running: return TaskStatus.Running;
            }
        }
        return TaskStatus.Success;
    }

    protected override void OnEnter()
    {
        
    }

    protected override void OnExit()
    {
        
    }

    public override void OnReset()
    {
        foreach (var c in children)
        {
            c.OnReset();
        }
    }
}
