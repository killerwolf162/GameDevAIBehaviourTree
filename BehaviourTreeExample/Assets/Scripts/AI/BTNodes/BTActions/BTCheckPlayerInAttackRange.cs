using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTCheckPlayerInAttackRange : BTBaseNode
{
    private Transform transform;
    private float attackRadius;
    private string BBtargetTransform;

    public BTCheckPlayerInAttackRange(string BBtargetTransform, Transform thisTransform, float attackRadius)
    {
        this.BBtargetTransform = BBtargetTransform;
        this.transform = thisTransform;
        this.attackRadius = attackRadius;
    }
    protected override void OnEnter()
    {
        Debug.Log("starting CheckPlayerInAttackrange");
    }
    protected override TaskStatus OnUpdate()
    {
        Transform target = blackboard.GetVariable<Transform>(BBtargetTransform);
        if (target == null) return TaskStatus.Success;

        if (Vector3.Distance(transform.position, target.position) < attackRadius)
        {
            blackboard.SetVariable(VariableNames.PLAYER_IN_ATTACK_RANGE, true);
            return TaskStatus.Success;
        }
        blackboard.SetVariable(VariableNames.PLAYER_IN_ATTACK_RANGE, false);
        return TaskStatus.Success;
    }
    protected override void OnExit()
    {
        Debug.Log("Exited CheckPlayerInAttackRange");
    }
}
