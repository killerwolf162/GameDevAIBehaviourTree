using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateStateText : MonoBehaviour
{

    [SerializeField] private Transform target;
    private Transform self;
    private float angleError = 1f; //in Degrees
    private float rotationSpeed = 180f;

    private void Start()
    {
        target = FindObjectOfType<Camera>().gameObject.transform;
        self = transform;
    }
    void Update()
    {
        float dot = Vector3.Dot(self.forward, (target.position - self.position).normalized);
        if(Utility.Remap(dot, 1, -1, 0, 180) > angleError)
        {
            self.rotation = Quaternion.RotateTowards(self.rotation, Quaternion.LookRotation((target.position - self.position) *-1), rotationSpeed * Time.deltaTime);
        }
    }
}
