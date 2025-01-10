using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTAttackPlayer : BTBaseNode
{
    public BTAttackPlayer()
    {

    }

    protected override void OnEnter()
    {
        Debug.Log("Entered player attack");
    }
    protected override TaskStatus OnUpdate() // placeholder code
    {
        Debug.Log("performing attack on player");
        return TaskStatus.Success;
    }

    protected override void OnExit()
    {
        Debug.Log("Exited player attack");
    }

}
