using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BTMoveToPosition : BTBaseNode
{
    private NavMeshAgent agent;
    private float moveSpeed;
    private float keepDistance;
    private Vector3 targetPosition;
    private string BBtargetPosition;

    public BTMoveToPosition(NavMeshAgent _agent, float _moveSpeed, string _BBtargetPosition, float _keepDistance)
    {
        this.agent = _agent;
        this.moveSpeed = _moveSpeed;
        this.BBtargetPosition = _BBtargetPosition;
        this.keepDistance = _keepDistance;
    }

    protected override void OnEnter()
    {
        agent.speed = moveSpeed;
        agent.stoppingDistance = keepDistance;
        targetPosition = blackboard.GetVariable<Vector3>(BBtargetPosition);
    }

    protected override TaskStatus OnUpdate()
    {
        if (agent == null) { return TaskStatus.Failed; }
        //if player in sight {return TaskStatus.Failed;}
        if (agent.pathPending) { return TaskStatus.Running; }
        if (agent.hasPath && agent.path.status == NavMeshPathStatus.PathInvalid) { return TaskStatus.Failed; }
        if (agent.pathEndPosition != targetPosition)
        {
            agent.SetDestination(targetPosition);
        }

        if (Vector3.Distance(agent.transform.position, targetPosition) <= keepDistance)
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Running;      
    }
    protected override void OnExit()
    {
    }
}

