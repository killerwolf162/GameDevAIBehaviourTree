using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rogue : MonoBehaviour
{

    [SerializeField] private float moveSpeed =3f;
    [SerializeField] private float moveDistance = 0.5f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float minDistanceToPlayer = 2f;
    [SerializeField] private float maxSmokeCooldown = 5f;
    [SerializeField] private float currSmokeCooldown = 0f;
    [SerializeField] private GameObject smokeGrenade;

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
        Blackboard blackboard = new Blackboard();
        blackboard.SetVariable(VariableNames.GUARD, FindAnyObjectByType<Guard>());
        blackboard.SetVariable(VariableNames.PLAYER, GameObject.FindWithTag("Player"));
        blackboard.SetVariable(VariableNames.TARGET, blackboard.GetVariable<GameObject>(VariableNames.PLAYER));

        #region RogueTree
        var FollowPlayerTree =
            new BTSequence(
                new BTCondition(() => { return CheckIfPlayerAlive(); }),
                new BTGenericAction(() => { blackboard.SetVariable(VariableNames.TARGET_POSITION, blackboard.GetVariable<GameObject>(VariableNames.TARGET).transform.position); 
                    Debug.Log(blackboard.GetVariable<Vector3>(VariableNames.PLAYER_POSITION)); }),
                new BTGenericAction(() => { animator.CrossFade("Walk Crouch", 0.2f, 0); }),
                new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, minDistanceToPlayer),
                new BTCondition(() => { return (Vector3.Distance(transform.position, blackboard.GetVariable<Vector3>(VariableNames.PLAYER_POSITION)) <= minDistanceToPlayer); }),
                new BTGenericAction(() => { animator.CrossFade("Crouch Idle", 0.2f, 0); }),
                new BTWait(1f)
                ) ;

        var ThrowSmokeGrenadeTree =
            new BTSequence(
                new BTCondition(() => { return CheckSmokeCooldown(); }),
                new BTGenericAction(() => { animator.CrossFade("Throw", 0.5f, 0); }),
                new BTWait(0.5f),
                new BTGenericAction(() => { blackboard.SetVariable(VariableNames.TARGET, blackboard.GetVariable<GameObject>(VariableNames.PLAYER)); }),
                new BTGenericAction(() => { Instantiate(smokeGrenade, blackboard.GetVariable<GameObject>(VariableNames.TARGET).transform.position, transform.rotation); }),
                new BTGenericAction(() => { currSmokeCooldown = maxSmokeCooldown; })
                ) ;

        var FindHidingPlaceTree = // make Ally find hiding spot
            new BTWait(1f);

        var DefendPlayerTree =
            new BTSequence(
                new BTGenericAction(()=>
                {
                    blackboard.SetVariable(VariableNames.PLAYER_IN_SIGHT, blackboard.GetVariable<Guard>(VariableNames.GUARD) //refactor if multiple guard sights need to be checked
                    .CheckLineOfSightToTarget(blackboard.GetVariable<GameObject>(VariableNames.TARGET)));
                }),
                new BTCondition(()=> { return blackboard.GetVariable<bool>(VariableNames.PLAYER_IN_SIGHT); }),
                FindHidingPlaceTree,
                ThrowSmokeGrenadeTree
                );

        var IdleTree =
            new BTSequence(
                new BTGenericAction(() => { Debug.Log("Going idle"); }),
                new BTGenericAction(() => { animator.CrossFade("Walk Crouch", 0.2f, 0); }),
                new BTWait(2f)
                ) ;

        #endregion
        tree = new BTSelector(DefendPlayerTree, FollowPlayerTree, IdleTree);
        tree.SetupBlackboard(blackboard);
    }

    private void FixedUpdate()
    {

        tree?.Tick();
    }

    public bool CheckIfPlayerAlive()
    {
        GameObject player = FindAnyObjectByType<Player>().gameObject;

        if (player != null)
            return true;
        else return false;
    }

    public bool CheckSmokeCooldown()
    {
        currSmokeCooldown -= Time.deltaTime;

        if (currSmokeCooldown <= 0)
            return true;         
        else
            return false;
    }
}
