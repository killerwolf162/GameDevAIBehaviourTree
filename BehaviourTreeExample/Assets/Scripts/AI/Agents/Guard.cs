using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class Guard : MonoBehaviour
{
    public float moveSpeed = 3;
    public float moveDistance = 0.1f;
    public float attackDistance = 1f;
    public float sightRange = 5;
    public float rotationSpeed = 180f;
    public Transform[] wayPoints;
    private BTBaseNode tree;
    private NavMeshAgent agent;
    private Animator animator;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        //Create your Behaviour Tree here!
        Blackboard blackboard = new Blackboard();
        blackboard.SetVariable(VariableNames.ENEMY_HEALTH, 100);
        blackboard.SetVariable(VariableNames.TARGET_POSITION, new Vector3(0,0,0));
        blackboard.SetVariable(VariableNames.CURRENT_PATROL_INDEX, -1);
        blackboard.SetVariable(VariableNames.TARGET, GameObject.FindWithTag("Player"));

        // Do we have a weapon? if not we move to weapon
        var FindWeaponTree =
            new BTSequence(
                new BTCondition(() => { return blackboard.GetVariable<GameObject>(VariableNames.CURRENT_WEAPON) == null; }),
                new BTGenericAction(() =>
                {
                    var weapon = FindWeaponInRange(sightRange);
                    if (weapon != null)
                    {
                        blackboard.SetVariable<GameObject>(VariableNames.TARGET_WEAPON, weapon);
                    }
                }),
                new BTGenericAction(() => { blackboard.SetVariable<Vector3>(VariableNames.TARGET_POSITION, blackboard.GetVariable<GameObject>(VariableNames.TARGET_WEAPON).transform.position); }),
                new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, moveDistance),
                new BTGenericAction(() => { blackboard.SetVariable<GameObject>(VariableNames.CURRENT_WEAPON, blackboard.GetVariable<GameObject>(VariableNames.TARGET_WEAPON)); })
                );

        // Patrol, Wait then rotate towards new target and patrol again
        var PatrolTree = 
            new BTRepeater(wayPoints.Length, //Repeat for each patrol waypoint
                new BTSequence(
                    new BTGenericAction(() => { animator.CrossFade("Walk", 0.1f, 0); }),
                    new BTGetNextPatrolPosition(wayPoints),
                    new BTRotateToPosition(transform, rotationSpeed, VariableNames.TARGET_POSITION, 5f),
                    new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, moveDistance),
                    new BTGenericAction(() => { animator.CrossFade("Idle", 0.2f, 0); }),
                    new BTWait(7f)
                   )
            );
        
        // Find Weapon, check line of Sight and move to target
        var ChaseTree =
            new BTSequence(
                new BTCondition<GameObject>((x) => { return CheckLineOfSightToTarget(x); }, blackboard.GetVariable<GameObject>(VariableNames.TARGET)),
                new BTAlwaysTrue(FindWeaponTree), // Reuse an existing tree but make it optional using the decorator
                new BTGenericAction(() => { blackboard.SetVariable<Vector3>(VariableNames.TARGET_POSITION, blackboard.GetVariable<GameObject>(VariableNames.TARGET).transform.position); }),
                new BTGenericAction(() => { animator.CrossFade("Walk", 0.2f, 0); }),
                new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, attackDistance)
            );

        tree = new BTSelector(ChaseTree, PatrolTree);
        tree.SetupBlackboard(blackboard);
    }

    public GameObject FindWeaponInRange(float range)
    {
        var weapons = GameObject.FindObjectsOfType<Weapon>();
        return weapons
            .ToList()
            .FindAll(x=> Vector3.Distance(x.transform.position, transform.position) <= range)
            .OrderBy(x => Vector3.Distance(transform.position, x.transform.position))
            .First()
            .gameObject;
    }

    public bool CheckLineOfSightToTarget(GameObject target)
    {
        Vector3 eyePosition = transform.position + new Vector3(0, 1.8f, 0);

        if(Physics.Raycast(eyePosition, target.transform.position - eyePosition, out RaycastHit hit, sightRange))
        {
            return hit.collider.gameObject == target;
        }
        return false;
    }

    private void FixedUpdate()
    {
        TaskStatus result = tree.Tick();
        //if(result != TaskStatus.Running)
        //{
        //    enabled = false;
        //}
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.yellow;
        //Handles.color = Color.yellow;
        //Vector3 endPointLeft = viewTransform.position + (Quaternion.Euler(0, -ViewAngleInDegrees.Value, 0) * viewTransform.transform.forward).normalized * SightRange.Value;
        //Vector3 endPointRight = viewTransform.position + (Quaternion.Euler(0, ViewAngleInDegrees.Value, 0) * viewTransform.transform.forward).normalized * SightRange.Value;

        //Handles.DrawWireArc(viewTransform.position, Vector3.up, Quaternion.Euler(0, -ViewAngleInDegrees.Value, 0) * viewTransform.transform.forward, ViewAngleInDegrees.Value * 2, SightRange.Value);
        //Gizmos.DrawLine(viewTransform.position, endPointLeft);
        //Gizmos.DrawLine(viewTransform.position, endPointRight);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

    }
}
