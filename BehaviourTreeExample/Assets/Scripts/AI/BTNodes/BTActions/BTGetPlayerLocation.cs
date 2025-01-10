using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTGetPlayerLocation : BTBaseNode
{

    private Transform transform;
    private float radius;
    private float maxDistance = 0;
    private LayerMask playerLayer;
    RaycastHit hit;

    public BTGetPlayerLocation(Transform transform, float radius, LayerMask playerLayer)
    {
        this.transform = transform;
        this.radius = radius;
        this.playerLayer = playerLayer;
    }

    protected override void OnEnter()
    {
        Debug.Log("starting player check");
    }

    protected override TaskStatus OnUpdate()
    {
        blackboard.SetVariable(VariableNames.PLAYER_TRANSFORM, transform);
        Debug.Log("player check succes, player location found");
        return TaskStatus.Success;
    }

    protected override void OnExit()
    {
        Debug.Log("exiting player check");
    }
}
