using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Guard : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3;
    [SerializeField] private float moveDistance = 0.5f;
    [SerializeField] private float sightRange = 5;
    [SerializeField] private float weaponSenseRange = 20;
    [SerializeField] private float attackDistance = 1f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float senseRadius = 10;
    [SerializeField] private int attackDamage = 2;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private GameObject weapon;
    public Transform[] wayPoints;
    private BTBaseNode tree;
    private NavMeshAgent agent;
    private Animator animator;
    private Transform playerTransform;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        weapon = FindObjectsOfType<Weapon>().ToList().First().gameObject;
        Debug.Log(weapon.name);
        playerTransform = FindAnyObjectByType<Player>().transform;
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        //Create your Behaviour Tree here!
        Blackboard blackboard = new Blackboard();
        blackboard.SetVariable(VariableNames.ENEMY_HEALTH, 100);
        blackboard.SetVariable(VariableNames.TARGET_POSITION, new Vector3(0, 0, 0));
        blackboard.SetVariable(VariableNames.CURRENT_PATROL_INDEX, -1);
        blackboard.SetVariable(VariableNames.TARGET, GameObject.FindWithTag("Player"));

        #region OwnTree
        //BTBaseNode patrol = new BTSequence(
        //    new BTVisualLog("moving to waypoint 1"),
        //    new BTMoveToPosition(agent, moveSpeed, VariableNames.WAYPOINT_1, stoppingDistance),
        //    new BTWait(1f),
        //    new BTVisualLog("moving to waypoint 2"),
        //    new BTMoveToPosition(agent, moveSpeed, VariableNames.WAYPOINT_2, stoppingDistance),
        //    new BTWait(1f),
        //    new BTVisualLog("moving to waypoint 3"),
        //    new BTMoveToPosition(agent, moveSpeed, VariableNames.WAYPOINT_3, stoppingDistance),
        //    new BTWait(1f),
        //    new BTVisualLog("moving to waypoint 4"),
        //    new BTMoveToPosition(agent, moveSpeed, VariableNames.WAYPOINT_4, stoppingDistance),
        //    new BTWait(1f)
        //    );

        // BTBaseNode followPlayerSequence = new BTSequence(
        //     new BTVisualLog("moving to player"),
        //     new BTMoveToPosition(agent, moveSpeed, VariableNames.PLAYER_POSITION, stoppingDistance)
        //     );



        //BTBaseNode getWeaponSequence = new BTSequence(
        //    new BTCondition(() => { return blackboard.GetVariable<GameObject>(VariableNames.CURRENT_WEAPON) == null; }),
        //    new BTVisualLog("moving to weapon"),
        //    new BTMoveToPosition(agent, moveSpeed, VariableNames.WEAPON_TRANSFORM, stoppingDistance),
        //    new BTVisualLog("picking up weapon"),
        //    new BTPickUpWeapon(weapon)
        //    ); 

        //BTBaseNode attackPlayerSequence = new BTSequence(
        //    new BTVisualLog("attacking player"),
        //    new BTAttackPlayer()
        //    );

        //BTBaseNode attackOrFollowPlayerSelector = new BTSelector(
        //    new BTConditional(attackPlayerSequence, blackboard.GetVariable<bool>(VariableNames.PLAYER_IN_ATTACK_RANGE)),
        //    followPlayerSequence);*/

        //BTBaseNode checkForWeaponSelector = new BTSelector(
        //    attackOrFollowPlayerSelector,
        //    getWeaponSequence
        //    );

        //BTBaseNode setPlayerAsTargetSequence = new BTSequence(
        //    new BTCheckPlayerInAttackRange(VariableNames.PLAYER_TRANSFORM, transform, attackRange),
        //    checkForWeaponSelector
        //    );

        //BTBaseNode PlayerInSightSelector = new BTSelector(
        //    new BTConditional(setPlayerAsTargetSequence, blackboard.GetVariable<bool>(VariableNames.PLAYER_IN_SIGHT))
        //    //patrol
        //    );

        //tree = new BTSequence(
        //    new BTGetPlayerLocation(transform, senseRadius, playerLayer),
        //    new BTCheckPlayerInSight(VariableNames.PLAYER_TRANSFORM, transform, sightRange),
        //    PlayerInSightSelector
        //    );
        #endregion

        #region ValentijnTree met aanpassingen
        // find weapon to attack player
        var FindWeaponTree =
            new BTSequence(
                new BTCondition(() => { return blackboard.GetVariable<GameObject>(VariableNames.CURRENT_WEAPON) == null; }),
                new BTGenericAction(() =>
                {
                    if (weapon != null)
                    {
                        Debug.Log("weapon found");
                        blackboard.SetVariable(VariableNames.TARGET_WEAPON, weapon);
                    }
                }),
                new BTGenericAction(() => { blackboard.SetVariable(VariableNames.TARGET_POSITION, blackboard.GetVariable<GameObject>(VariableNames.TARGET_WEAPON).transform.position); }),
                new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, moveDistance),
                new BTGenericAction(() => { blackboard.SetVariable(VariableNames.CURRENT_WEAPON, blackboard.GetVariable<GameObject>(VariableNames.TARGET_WEAPON)); })
                );

        // return weapon if no player in sight
        var ReturnWeaponTree =
            new BTSequence(
                new BTCondition(() => { return blackboard.GetVariable<GameObject>(VariableNames.CURRENT_WEAPON) != null; }),
                new BTGenericAction(() =>
                {
                    blackboard.SetVariable(VariableNames.TARGET_POSITION, blackboard.GetVariable<GameObject>(VariableNames.TARGET_WEAPON).transform.position);
                    Debug.Log("returning weapon to storage");
                }),
                new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, moveDistance),
                new BTGenericAction(() => { blackboard.SetVariable<GameObject>(VariableNames.CURRENT_WEAPON, null); })
                );


        // Patrol, Wait then rotate towards new target and patrol again
        var PatrolTree =
            new BTRepeater(wayPoints.Length, //Repeat for each patrol waypoint
                new BTSequence(
                    new BTAlwaysTrue(ReturnWeaponTree),
                    new BTGenericAction(() => { animator.CrossFade("Walk", 0.1f, 0); }),
                    new BTGetNextPatrolPosition(wayPoints),
                    new BTRotateToPosition(transform, rotationSpeed, VariableNames.TARGET_POSITION, 5f),
                    new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, moveDistance),
                    new BTGenericAction(() => { animator.CrossFade("Idle", 0.2f, 0); }),
                    new BTWait(2f)
                   )
            );

        // attack sequence
        var AttackTree =
            new BTSequence(
                new BTGenericAction(() => { animator.CrossFade("Kick", 0.5f, 0); }),
                new BTGenericAction(() => { blackboard.GetVariable<GameObject>(VariableNames.TARGET).GetComponent<Player>().TakeDamage(this.gameObject, attackDamage); }),
                new BTWait(1f)
                );

        // Find Weapon, check line of Sight and move to target
        var ChaseTree =
            new BTSequence(
                new BTCondition<GameObject>((x) => { return CheckLineOfSightToTarget(x); }, blackboard.GetVariable<GameObject>(VariableNames.TARGET)),
                new BTAlwaysTrue(FindWeaponTree), // Reuse an existing tree but make it optional using the decorator
                new BTGenericAction(() => { blackboard.SetVariable(VariableNames.TARGET_POSITION, blackboard.GetVariable<GameObject>(VariableNames.TARGET).transform.position); }),
                new BTGenericAction(() => { animator.CrossFade("Walk", 0.2f, 0); }),
                new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, attackDistance),
                new BTCondition(() =>{ return (Vector3.Distance(transform.position, blackboard.GetVariable<Vector3>(VariableNames.TARGET_POSITION)) <= attackDistance); }),
                new BTAlwaysTrue(AttackTree)
                );
        

        #endregion
        tree = new BTSelector(ChaseTree, PatrolTree);
        tree.SetupBlackboard(blackboard);
    }


    private void FixedUpdate()
    {
        TaskStatus result = tree.Tick();
    }

    public bool CheckLineOfSightToTarget(GameObject target)
    {
        Vector3 eyePosition = transform.position + new Vector3(0, 1.8f, 0);

        if (Physics.Raycast(eyePosition, target.transform.position - eyePosition, out RaycastHit hit, sightRange))
        {
            return hit.collider.gameObject == target;
        }
        return false;
    }
}
