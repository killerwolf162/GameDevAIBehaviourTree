using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class Guard : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3;
    [SerializeField] private float moveDistance = 0.5f;
    [SerializeField] private float sightRange = 5;
    [SerializeField] private float maxSightRange = 5;
    [SerializeField] private float minSightRange = 0.5f;
    [SerializeField] private float attackDistance = 1f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private GameObject weapon;
    [SerializeField] private TextMeshProUGUI stateText;
    public Transform[] wayPoints;
    private BTBaseNode tree;
    private NavMeshAgent agent;
    private Animator animator;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        weapon = FindObjectsOfType<Weapon>().ToList().First().gameObject;
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        //Create your Behaviour Tree here!
        Blackboard blackboard = new Blackboard();
        blackboard.SetVariable(VariableNames.ENEMY_HEALTH, 100);
        blackboard.SetVariable(VariableNames.CURRENT_PATROL_INDEX, -1);
        blackboard.SetVariable(VariableNames.PLAYER, GameObject.FindWithTag("Player"));
        blackboard.SetVariable(VariableNames.TARGET, blackboard.GetVariable<GameObject>(VariableNames.PLAYER));

        #region OwnTree
        //BTBaseNode patrol = new BTSequence(
        //    new BTMoveToPosition(agent, moveSpeed, VariableNames.WAYPOINT_1, stoppingDistance),
        //    new BTWait(1f),
        //    new BTMoveToPosition(agent, moveSpeed, VariableNames.WAYPOINT_2, stoppingDistance),
        //    new BTWait(1f),
        //    new BTMoveToPosition(agent, moveSpeed, VariableNames.WAYPOINT_3, stoppingDistance),
        //    new BTWait(1f),
        //    new BTMoveToPosition(agent, moveSpeed, VariableNames.WAYPOINT_4, stoppingDistance),
        //    new BTWait(1f)
        //    );

        // BTBaseNode followPlayerSequence = new BTSequence(
        //     new BTMoveToPosition(agent, moveSpeed, VariableNames.PLAYER_POSITION, stoppingDistance)
        //     );



        //BTBaseNode getWeaponSequence = new BTSequence(
        //    new BTCondition(() => { return blackboard.GetVariable<GameObject>(VariableNames.CURRENT_WEAPON) == null; }),
        //    new BTMoveToPosition(agent, moveSpeed, VariableNames.WEAPON_TRANSFORM, stoppingDistance),
        //    new BTPickUpWeapon(weapon)
        //    ); 

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

        #region modified ValentijnTree
        // Move to weapon storage to get weapon
        var FindWeaponTree =
            new BTSequence(
                new BTCondition(() => { return blackboard.GetVariable<GameObject>(VariableNames.CURRENT_WEAPON) == null; }),
                new BTGenericAction(() =>
                {
                    if (weapon != null)
                        blackboard.SetVariable(VariableNames.TARGET_WEAPON, weapon);
                }),
                new BTGenericAction(() => { blackboard.SetVariable(VariableNames.TARGET_POSITION, blackboard.GetVariable<GameObject>(VariableNames.TARGET_WEAPON).transform.position); }),
                new BTGenericAction(() => { stateText.SetText("Getting Weapon"); }),
                new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, moveDistance),
                new BTGenericAction(() => { blackboard.SetVariable(VariableNames.CURRENT_WEAPON, blackboard.GetVariable<GameObject>(VariableNames.TARGET_WEAPON)); })
                );

        // Return weapon if no player is targeted
        var ReturnWeaponTree =
            new BTSequence(
                new BTCondition(() => { return blackboard.GetVariable<GameObject>(VariableNames.CURRENT_WEAPON) != null; }),
                new BTGenericAction(() => { blackboard.SetVariable(VariableNames.TARGET_POSITION, blackboard.GetVariable<GameObject>(VariableNames.TARGET_WEAPON).transform.position); }),
                new BTGenericAction(() => { stateText.SetText("returning weapon to storage"); }),
                new BTGenericAction(() => { animator.CrossFade("Walk", 0.1f, 0); }),
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
                    new BTGenericAction(() => { stateText.SetText("moving to next waypoint"); }),
                    new BTRotateToPosition(transform, rotationSpeed, VariableNames.TARGET_POSITION, 5f),
                    new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, moveDistance),
                    new BTGenericAction(() => { animator.CrossFade("Idle", 0.2f, 0); }),
                    new BTWait(1f)
                   )
            );

        // Attack sequence, check if player is alive and in range then attack
        var AttackTree =
            new BTSequence(
                new BTCondition(() => { return CheckIfPlayerAlive(); }), // Check if player isnt null
                new BTGenericAction(() => { blackboard.SetVariable(VariableNames.PLAYER_TRANSFORM, blackboard.GetVariable<GameObject>(VariableNames.PLAYER).transform); }), // Reset player transform
                new BTCondition(() =>
                {
                    return CheckIfPLayerIsTarget(
                        blackboard.GetVariable<Vector3>(VariableNames.TARGET_POSITION),
                        blackboard.GetVariable<Transform>(VariableNames.PLAYER_TRANSFORM).position);
                }),
                new BTGenericAction(() => { stateText.SetText("attacking player"); }),
                new BTGenericAction(() => { animator.CrossFade("Kick", 0.3f, 0); }),
                new BTWait(0.5f),
                new BTGenericAction(() => { blackboard.SetVariable(VariableNames.TARGET_POSITION, blackboard.GetVariable<GameObject>(VariableNames.TARGET).transform.position); }),
                new BTCondition(() => { return (Vector3.Distance(transform.position, blackboard.GetVariable<Vector3>(VariableNames.TARGET_POSITION)) <= attackDistance); }),
                new BTGenericAction(() => { blackboard.GetVariable<GameObject>(VariableNames.TARGET).GetComponent<Player>().TakeDamage(this.gameObject, attackDamage); }),
                new BTGenericAction(() => { animator.CrossFade("Idle", 0.4f, 0); }),
                new BTWait(1f)
                );

        // Get Weapon, check line of Sight and move to target
        var ChaseTree =
            new BTSequence(
                new BTCondition<GameObject>((x) => { return CheckLineOfSightToTarget(x); }, blackboard.GetVariable<GameObject>(VariableNames.TARGET)),
                new BTAlwaysTrue(FindWeaponTree), // Reuse an existing tree but make it optional using the decorator
                new BTGenericAction(() => { blackboard.SetVariable(VariableNames.TARGET_POSITION, blackboard.GetVariable<GameObject>(VariableNames.TARGET).transform.position); }),
                new BTGenericAction(() => { animator.CrossFade("Walk", 0.2f, 0); }),
                new BTGenericAction(() => { stateText.SetText("chasing player"); }),
                new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, attackDistance),
                new BTCondition(() => { return (Vector3.Distance(transform.position, blackboard.GetVariable<Vector3>(VariableNames.TARGET_POSITION)) <= attackDistance); }),
                AttackTree
                );


        #endregion
        tree = new BTSelector(ChaseTree, PatrolTree);
        tree.SetupBlackboard(blackboard);
    }


    private void FixedUpdate()
    {
        InsideSmoke();
        TaskStatus result = tree.Tick();
    }

    public bool CheckLineOfSightToTarget(GameObject target)
    {
        Vector3 eyePosition = transform.position + new Vector3(0, 1.8f, 0);

        if (Physics.Raycast(eyePosition, target.transform.position - eyePosition, out RaycastHit hit, sightRange))
        {
            if (hit.collider.gameObject.tag == "Player") { return hit.collider.gameObject == target; }
            else { return false; }
        }
        else { return false; }
    }
    public bool CheckIfPLayerIsTarget(Vector3 targetPos, Vector3 playerPos)
    {
        if (targetPos == playerPos) { return true; }
        else { return false; }
    }

    public bool CheckIfPlayerAlive()
    {
        if (FindAnyObjectByType<Player>().gameObject.activeSelf) { return true; }
        else { return false; }
    }

    public void InsideSmoke()
    {
        List<SmokeGrenade> grenades = FindObjectsOfType<SmokeGrenade>().ToList();
        grenades.OrderBy(x => Vector3.Distance(x.transform.position, transform.position));
        if (grenades.Count > 0)
        {
            if (Vector3.Distance(grenades.First().transform.position, transform.position) < grenades.First().smokeRadius) { sightRange = minSightRange; }
            else { sightRange = maxSightRange; }
            grenades.Clear();
        }
        else { sightRange = maxSightRange; }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;

    //    //Check if there has been a hit yet
    //    if (hitDetected)
    //    {
    //        //Draw a Ray forward from GameObject toward the hit
    //        Gizmos.DrawRay(transform.position, transform.forward * hit.distance);
    //        //Draw a cube that extends to where the hit exists
    //        Gizmos.DrawWireCube(transform.position + transform.forward * hit.distance, transform.localScale);
    //    }
    //    //If there hasn't been a hit yet, draw the ray at the maximum distance
    //    else
    //    {
    //        //Draw a Ray forward from GameObject toward the maximum distance
    //        Gizmos.DrawRay(transform.position, transform.forward * attackDistance);
    //        //Draw a cube at the maximum distance
    //        Gizmos.DrawWireCube(transform.position + transform.forward * attackDistance, transform.localScale);
    //    }
    //}
}
