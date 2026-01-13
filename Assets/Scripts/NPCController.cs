using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.Animations.Rigging;

public class NPCController : MonoBehaviour
{
    [Header("Animation")]
    private Animator animator;

    [Header("Movement")]
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;

    [Header("Speed Settings")]
    public float walkSpeed = 4f;
    public float runSpeed = 5f;
    public bool shouldRun = false;

    [Header("Detection")]
    public GameObject ChestRig;
    public GameObject RightArmRig;
    public GameObject LeftArmRig;

    private NavMeshAgent agent;
    private FieldOfView fov;
    private WeaponEquip weaponEquip;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        fov = GetComponent<FieldOfView>();
        weaponEquip = GetComponent<WeaponEquip>();

        agent.speed = shouldRun ? runSpeed : walkSpeed;

        if (waypoints.Length > 0)
        {
            GoToNextWaypoint();
        }
    }

    void Update()
    {
        float speedPercent = agent.velocity.magnitude / runSpeed;
        animator.SetFloat("speed", speedPercent);

        if (fov != null && fov.canSeePlayer)
        {
            PlayerDetected();
        }
        else
        {
            PlayerNotDetected();
        }
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        agent.destination = waypoints[currentWaypointIndex].position;
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    void PlayerDetected()
    {
        agent.speed = 0;
        if (!weaponEquip.isEquipped)
            weaponEquip.ToggleEquip();
        ChestRig.GetComponent<MultiAimConstraint>().weight = 1;
        RightArmRig.GetComponent<TwoBoneIKConstraint>().weight = 1;
        LeftArmRig.GetComponent<TwoBoneIKConstraint>().weight = 1;
    }

    void PlayerNotDetected()
    {
        agent.speed = shouldRun ? runSpeed : walkSpeed;
        if (weaponEquip.isEquipped)
        {
            weaponEquip.ToggleEquip();
        }
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToNextWaypoint();
        ChestRig.GetComponent<MultiAimConstraint>().weight = 0;
        RightArmRig.GetComponent<TwoBoneIKConstraint>().weight = 0;
        LeftArmRig.GetComponent<TwoBoneIKConstraint>().weight = 0;
    }
}