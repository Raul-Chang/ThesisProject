using UnityEngine;
using UnityEngine.AI; 

public class EnemyAI : MonoBehaviour
{
    public float visionRange = 5f;       
    public float robberyDistance = 1.5f; 
    public Transform[] patrolPoints;      
    public float waitTime = 2f;           

    private Transform player;
    private NavMeshAgent agent;
    private int currentPoint = 0;
    private float waitTimer;
    private bool chasing = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        GoToNextPoint();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

       
        if (distanceToPlayer <= visionRange)
        {
            chasing = true;
            agent.SetDestination(player.position);
        }
        else if (chasing && distanceToPlayer > visionRange * 1.5f) 
        {
            chasing = false;
            GoToNextPoint();
        }

        
        if (!chasing && !agent.pathPending && agent.remainingDistance < 0.3f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                GoToNextPoint();
                waitTimer = 0f;
            }
        }

       
        if (chasing && distanceToPlayer <= robberyDistance)
        {
            RobPlayer();
        }
    }

    void GoToNextPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPoint].position);
        currentPoint = (currentPoint + 1) % patrolPoints.Length;
    }

    void RobPlayer()
    {
        Debug.Log("¡El chorro te robó!");
        
    }
}