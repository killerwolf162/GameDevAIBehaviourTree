using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTPickUpWeapon : BTBaseNode
{


    public BTPickUpWeapon()
    {

    }

    protected override void OnEnter()
    {
        Debug.Log("starting Pickup weapon");
    }

    protected override TaskStatus OnUpdate()
    {
        Debug.Log("PickUpWeapon Succes");
        return TaskStatus.Success;
    }

    protected override void OnExit()
    {
        Debug.Log("Exiting Pickup weapon");
    }
}
