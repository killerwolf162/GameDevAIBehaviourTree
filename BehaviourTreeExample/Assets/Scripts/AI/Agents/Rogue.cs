using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using TMPro;

public class Rogue : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float minDistanceToPlayer = 0.5f;
    [SerializeField] private float maxSmokeCooldown = 5f;
    [SerializeField] private float currSmokeCooldown = 0f;
    [SerializeField] private GameObject smokeGrenade;
    [SerializeField] private TextMeshProUGUI stateText;

    public Transform[] wayPoints;
    private BTBaseNode tree;
    private NavMeshAgent agent;
    private Animator animator;
    private GameObject player;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        player = GameObject.FindWithTag("Player");
    }

    private void Start()
    {
        Blackboard blackboard = new Blackboard();
        blackboard.SetVariable(VariableNames.GUARD, FindAnyObjectByType<Guard>());
        blackboard.SetVariable(VariableNames.PLAYER, GameObject.FindWithTag("Player"));
        blackboard.SetVariable(VariableNames.TARGET, blackboard.GetVariable<GameObject>(VariableNames.PLAYER));

        #region RogueTree
        var FollowPlayerTree = // Follow player if not dead
            new BTSequence(
                new BTCondition(() => { return CheckIfPlayerAlive(); }),
                new BTGenericAction(() =>
                {
                    blackboard.SetVariable(VariableNames.TARGET_POSITION, blackboard.GetVariable<GameObject>(VariableNames.PLAYER).transform.position);
                }),
                new BTGenericAction(() => { animator.CrossFade("Crouch Idle", 0.4f, 0); }),
                new BTGenericAction(() => { stateText.SetText("Waiting for player to move"); }),
                new BTCondition(() => { return (Vector3.Distance(transform.position, blackboard.GetVariable<Vector3>(VariableNames.PLAYER_POSITION)) >= minDistanceToPlayer); }),
                new BTGenericAction(() => { animator.CrossFade("Walk Crouch", 0.2f, 0); }),
                new BTGenericAction(() => { stateText.SetText("Following player"); }),
                new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, 1f)
                );

        var ThrowSmokeGrenadeTree = // check if smoke available, then throw smoke
            new BTSequence(
                new BTCondition(() => { return CheckSmokeCooldown(); }),
                new BTGenericAction(() => { stateText.SetText("Throwing smokegrenade"); }),
                new BTWait(0.5f),
                new BTGenericAction(() => { animator.CrossFade("Throw", 0.5f, 0); }),
                new BTWait(1f),
                new BTGenericAction(() => { blackboard.SetVariable(VariableNames.TARGET, blackboard.GetVariable<GameObject>(VariableNames.PLAYER)); }),
                new BTGenericAction(() => { Instantiate(smokeGrenade, blackboard.GetVariable<GameObject>(VariableNames.TARGET).transform.position, transform.rotation); }),
                new BTGenericAction(() => { currSmokeCooldown = maxSmokeCooldown; })
                );

        var FindHidingPlaceTree = // Move to closest hidingspot
            new BTSequence(
                new BTGenericAction(() => { stateText.SetText("Finding Hidingspot"); }),
                new BTGenericAction(() =>
                {
                    var hidingSpot = FindClosestHidngSpot(); 
                    if (hidingSpot != null)
                    {
                        blackboard.SetVariable(VariableNames.TARGET, hidingSpot);
                    }
                }),
                new BTGenericAction(() => { blackboard.SetVariable(VariableNames.TARGET_POSITION, blackboard.GetVariable<GameObject>(VariableNames.TARGET).transform.position); }),
                new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, minDistanceToPlayer)
                ) ;


        var DefendPlayerTree = // Check if guard is targeting player, find spot to hide, throw smoke
            new BTSequence(
                new BTGenericAction(() =>
                {
                    blackboard.SetVariable(VariableNames.PLAYER_IN_SIGHT, blackboard.GetVariable<Guard>(VariableNames.GUARD) //refactor if multiple guard sights need to be checked
                    .CheckLineOfSightToTarget(blackboard.GetVariable<GameObject>(VariableNames.TARGET)));
                }),
                new BTCondition(() => { return blackboard.GetVariable<bool>(VariableNames.PLAYER_IN_SIGHT); }),

                FindHidingPlaceTree,
                ThrowSmokeGrenadeTree
                );

        var IdleTree = // if player is dead, stand idle
            new BTSequence(
                new BTGenericAction(() => { stateText.SetText("Going Idle"); }),
                new BTGenericAction(() => { animator.CrossFade("Crouch Idle", 0.2f, 0); }),
                new BTWait(2f)
                );

        #endregion
        tree = new BTSelector(DefendPlayerTree, FollowPlayerTree, IdleTree);
        tree.SetupBlackboard(blackboard);
    }

    private void FixedUpdate()
    {
        tree?.Tick();
        currSmokeCooldown -= Time.deltaTime;
    }

    public GameObject FindClosestHidngSpot()
    {
        var hidingSpots = FindObjectsOfType<HidingSpot>();
        return hidingSpots
            .ToList()
            .OrderBy(x => Vector3.Distance(x.transform.position, transform.position))
            .First()
            .gameObject;
    }

    public bool CheckIfPlayerAlive()
    {
        if (player.activeSelf)
        {
            return true;
        }
        else return false;
    }

    public bool CheckSmokeCooldown()
    {
        if (currSmokeCooldown <= 0)
            return true;
        else
            return false;
    }
}
