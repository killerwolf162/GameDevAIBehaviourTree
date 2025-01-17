using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System;

public class TankAI : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private float keepDistance = 30f;
    [SerializeField] private float sightRange = 100;
    [SerializeField] private float audioRange = 30;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float errorAngle = 0.1f;
    [SerializeField] private float shootingCooldown = 0f;
    [SerializeField] private float maxSchootingCooldown = 5f;
    [SerializeField] private TextMeshProUGUI stateText;

    [SerializeField] private GameObject turret;
    [SerializeField] private GameObject shell;
    [SerializeField] private GameObject player;
    [SerializeField] private Quaternion angleOffSet;

    public Transform[] wayPoints;
    private BTBaseNode tree;
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        Blackboard blackboard = new Blackboard();
        blackboard.SetVariable(VariableNames.ENEMY_HEALTH, 100);
        blackboard.SetVariable(VariableNames.CURRENT_PATROL_INDEX, -1);
        blackboard.SetVariable(VariableNames.PLAYER, player);
        blackboard.SetVariable(VariableNames.TARGET, blackboard.GetVariable<GameObject>(VariableNames.PLAYER));
        blackboard.SetVariable(VariableNames.TURRET_TRANSFORM, turret.transform);
        blackboard.SetVariable(VariableNames.KEEP_DISTANCE, keepDistance);

        #region Tank-BehaviourTree

        var PatrolTree =
            new BTRepeater(wayPoints.Length, //Repeat for each patrol waypoint
                new BTSequence(
                    new BTGenericAction(()=> { keepDistance = 20f; }),
                    new BTGenericAction(() => { blackboard.SetVariable(VariableNames.KEEP_DISTANCE, keepDistance); }),
                    new BTGetNextPatrolPosition(wayPoints),
                    new BTGenericAction(() => { if(stateText != null) stateText.SetText("Patrolling: Going to next waypoint"); }),
                    new BTRotateToPosition(transform, rotationSpeed, VariableNames.TARGET_POSITION, 1f),
                    new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, VariableNames.KEEP_DISTANCE),
                    new BTWait(1f)
                   )
            );

        var AttackTree =
            new BTSequence(
                new BTCondition<GameObject>((x) => { return CheckLineOfSightToTarget(x); }, player),
                new BTGenericAction(() => { if (stateText != null) stateText.SetText("Attacking target"); }),
                new BTGenericAction(()=> { blackboard.SetVariable(VariableNames.TARGET_POSITION, player.transform.position); }),
                new BTRotateToPosition(turret.transform, rotationSpeed, VariableNames.TARGET_POSITION, errorAngle),
                new BTCondition(() => { return ShootingOfCooldown(); }),
                new BTGenericAction(() => { if (stateText != null) stateText.SetText("Shooting at target"); }),
                new BTGenericAction(() => { ShootBullet(); })              
                );

        var ChaseTree =
            new BTSequence(
                new BTCondition<GameObject>((x) => { return CheckLineOfSightToTarget(x); }, player),
                new BTGenericAction(() => { blackboard.SetVariable(VariableNames.TARGET_POSITION, player.transform.position); }),
                new BTGenericAction(()=> { keepDistance = 40; }),
                new BTGenericAction(() => { blackboard.SetVariable(VariableNames.KEEP_DISTANCE, keepDistance); }),
                new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, VariableNames.KEEP_DISTANCE),
                new BTGenericAction(() => { if (stateText != null) stateText.SetText("In Position"); })
                );

        var SearchTree =
            new BTSequence(
                new BTCondition<GameObject>((x) => { return PlayerInAudioRange(x); }, player),
                new BTGenericAction(() => { blackboard.SetVariable(VariableNames.TARGET_POSITION, player.transform.position); }),
                new BTGenericAction(() => { keepDistance = 2; }),
                new BTGenericAction(() => { blackboard.SetVariable(VariableNames.KEEP_DISTANCE, keepDistance); }),
                new BTGenericAction(() => { if (stateText != null) stateText.SetText("Hearing sound, searching for target"); }),
                new BTRotateToPosition(turret.transform, rotationSpeed, VariableNames.TARGET_POSITION, errorAngle),
                new BTMoveToPosition(agent, moveSpeed, VariableNames.TARGET_POSITION, VariableNames.KEEP_DISTANCE),
                new BTGenericAction(() => { if (stateText != null) stateText.SetText("Moving to sound"); })
                );
        #endregion

        tree = new BTSelector(AttackTree, ChaseTree, SearchTree, PatrolTree);
        tree.SetupBlackboard(blackboard);
    }

    public void ShootBullet()
    {
        GameObject _shell = Instantiate(shell, turret.transform.position, turret.transform.rotation);
        _shell.GetComponent<ShellScript>().parent = this.gameObject;
        shootingCooldown = maxSchootingCooldown;
    }

    public bool PlayerInPosition()
    {
        return true;
    }

    public bool ShootingOfCooldown()
    {
        if (shootingCooldown < 0) { return true; }
        else { return false; }
    }

    public bool CheckLineOfSightToTarget(GameObject target)
    {
        Vector3 eyePosition = turret.transform.position + new Vector3(0, 0, 0);
        if (Physics.Raycast(eyePosition, target.transform.position - eyePosition, out RaycastHit hit, sightRange))
        {
            if (hit.collider.gameObject.tag == "Player" && Vector3.Distance(eyePosition, target.transform.position) < sightRange) { Debug.Log("hit"); return true; }
            else { Debug.Log("miss"); return false; }
        }
        else { Debug.Log("miss"); return false; }
    }

    public bool PlayerInAudioRange(GameObject target)
    {
        if (Vector3.Distance(transform.position, target.transform.position) < audioRange) { Debug.Log("In audio range"); return target; }
        else { Debug.Log("Not in audio range"); return false; }
    }

    private void FixedUpdate()
    {
        shootingCooldown -= Time.deltaTime;
        tree?.Tick();
    }
}
