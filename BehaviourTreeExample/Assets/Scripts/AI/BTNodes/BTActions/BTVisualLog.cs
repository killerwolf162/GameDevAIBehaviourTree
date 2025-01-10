using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTVisualLog : BTBaseNode
{
    private string logMessage;

    public BTVisualLog(string logMessage)
    {
        this.logMessage = logMessage;
    }
    protected override void OnEnter()
    {
        blackboard.SetVariable<string>(logMessage, "no message");
    }
    protected override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}
