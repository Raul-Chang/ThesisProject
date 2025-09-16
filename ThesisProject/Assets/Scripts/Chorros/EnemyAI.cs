using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public float visionRange = 5f;
    public float robberyDistance = 1.5f;
    public float waitTime = 2f;

    private Transform player;
    private NavMeshAgent agent;
    private float waitTimer;
    private bool chasing = false;

    private Waypoint currentWaypoint;

    public GameObject defeatUI;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();

       
        currentWaypoint = FindClosestWaypoint();
        GoToNextWaypoint();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

       
        if (distanceToPlayer <= visionRange)
        {
            chasing = true;
        }
        else if (chasing && distanceToPlayer > visionRange * 1.5f)
        {
            chasing = false;
            GoToNextWaypoint();
        }

        if (chasing)
        {
            agent.SetDestination(player.position);
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.3f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                GoToNextWaypoint();
                waitTimer = 0f;
            }
        }

        if (chasing && distanceToPlayer <= robberyDistance)
        {
            RobPlayer();
        }
    }

    void GoToNextWaypoint()
    {
        if (currentWaypoint == null || currentWaypoint.connectedWaypoints.Length == 0) return;

        
        Waypoint nextWaypoint = currentWaypoint.connectedWaypoints[
            Random.Range(0, currentWaypoint.connectedWaypoints.Length)
        ];

        currentWaypoint = nextWaypoint;
        agent.SetDestination(currentWaypoint.transform.position);
    }

    Waypoint FindClosestWaypoint()
    {
        Waypoint[] allWaypoints = FindObjectsOfType<Waypoint>();
        Waypoint closest = null;
        float minDist = Mathf.Infinity;

        foreach (var wp in allWaypoints)
        {
            float dist = Vector3.Distance(transform.position, wp.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = wp;
            }
        }

        return closest;
    }

    void RobPlayer()
    {
        Debug.Log("¡El chorro te robó!");
        defeatUI.SetActive(true);
        Menu.Instance.PauseGame();
    }
}