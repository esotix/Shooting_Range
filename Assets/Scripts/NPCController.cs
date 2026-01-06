using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    [Header("Animation")]
    private Animator animator;

    [Header("Movement")]
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;

    [Header("Speed Settings")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 5f;
    public bool shouldRun = false;

    private NavMeshAgent agent;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

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

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextWaypoint();
        }
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        agent.destination = waypoints[currentWaypointIndex].position;
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }
}