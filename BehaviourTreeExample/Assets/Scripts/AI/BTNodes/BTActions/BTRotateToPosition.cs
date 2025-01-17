using UnityEngine;

public class BTRotateToPosition : BTBaseNode
{
    private float rotationSpeed;
    private Vector3 targetPosition;
    private string BBtargetPosition;
    private Transform self;
    private float angleError = 1f; //in Degrees

    public BTRotateToPosition(Transform _self, float _rotationSpeed, string _BBtargetPosition, float _angleError)
    {        
        this.self = _self;
        this.rotationSpeed = _rotationSpeed;
        this.BBtargetPosition = _BBtargetPosition;            
        this.angleError = _angleError;
    }

    protected override void OnEnter()
    {
        targetPosition = blackboard.GetVariable<Vector3>(BBtargetPosition);
    }

    protected override TaskStatus OnUpdate()
    {
        
        float dot = Vector3.Dot(self.forward, (targetPosition - self.position).normalized);
        if (Utility.Remap(dot, 1, -1, 0, 180) > angleError)
        {
            self.transform.rotation = Quaternion.RotateTowards(self.transform.rotation, Quaternion.LookRotation(targetPosition - self.position), rotationSpeed * Time.deltaTime);
            return TaskStatus.Running;
        }
        return TaskStatus.Success;
    }
}
