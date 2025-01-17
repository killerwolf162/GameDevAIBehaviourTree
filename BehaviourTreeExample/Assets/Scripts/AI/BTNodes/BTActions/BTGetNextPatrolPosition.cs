using UnityEngine;

public class BTGetNextPatrolPosition : BTBaseNode
{
    private Transform[] wayPoints;
    public BTGetNextPatrolPosition(Transform[] _wayPoints)
    {
        this.wayPoints = _wayPoints;
    }

    protected override void OnEnter()
    {
        int currentIndex = blackboard.GetVariable<int>(VariableNames.CURRENT_PATROL_INDEX);
        currentIndex++;
        if (currentIndex >= wayPoints.Length)
        {
            currentIndex = 0;
        }
        blackboard.SetVariable(VariableNames.CURRENT_PATROL_INDEX, currentIndex);
        blackboard.SetVariable(VariableNames.TARGET_POSITION, wayPoints[currentIndex].position);
    }

    protected override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}

