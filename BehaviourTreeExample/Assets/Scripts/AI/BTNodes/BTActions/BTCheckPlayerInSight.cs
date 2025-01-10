using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTCheckPlayerInSight : BTBaseNode
{
    private Transform transform;
    private float sightRadius;
    private string BBtargetTransform;

    public BTCheckPlayerInSight(string BBtargetTransform, Transform thisTransform, float sightRadius)
    {
        this.BBtargetTransform = BBtargetTransform;
        this.transform = thisTransform;
        this.sightRadius = sightRadius;
    }

    protected override void OnEnter()
    {
        Debug.Log("entered check player in sight");
    }
    protected override TaskStatus OnUpdate()
    {
        Debug.Log("started check player in sight");

        Transform target = null;

        //if (target == null)  // commented null check to simulate target in sight
        //{
        //    Debug.Log("player in sight check finished, target == null");
        //    return TaskStatus.Success;
        //}


        Debug.Log("player in sight check finished, player in sight");
        blackboard.SetVariable(VariableNames.PLAYER_IN_SIGHT, true); // simulate if player was in guard sight
        
        return TaskStatus.Success;
    }
    protected override void OnExit()
    {
        Debug.Log("Player in sight: " + blackboard.GetVariable<bool>(VariableNames.PLAYER_IN_SIGHT));
        Debug.Log("exited check player in sight");
    }
}
