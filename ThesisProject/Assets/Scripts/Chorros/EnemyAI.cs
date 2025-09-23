using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Configuración de visión y robo")]
    public float visionRange = 5f;
    public float robberyDistance = 1.5f;
    public float waitTime = 2f;

    [Header("Anticipación Visual")]
    [Tooltip("Distancia a la que se activa el material de alerta (debe ser mayor que visionRange).")]
    public float alertRange = 8f;
    public Material alertMat;          
    private Material originalMat;      
    private Renderer rend;            

    private Transform player;
    private NavMeshAgent agent;
    private float waitTimer;
    private bool chasing = false;

    [Header("Waypoints")]
    private Waypoint currentWaypoint;
    private Waypoint[] allWaypoints;
    private Waypoint startWaypoint;

    void Start()
    {
       
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("EnemyAI: No se encontró ningún objeto con tag 'Player'");
            return;
        }
        player = playerObj.transform;

       
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("EnemyAI: No se encontró componente NavMeshAgent en " + gameObject.name);
            return;
        }

       
        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            originalMat = rend.material;
        }
        else
        {
            Debug.LogWarning("EnemyAI: No se encontró Renderer en hijos de " + gameObject.name);
        }

       
        allWaypoints = FindObjectsOfType<Waypoint>();
        Debug.Log("EnemyAI: Waypoints encontrados = " + allWaypoints.Length);

        currentWaypoint = FindClosestWaypoint();
        startWaypoint = currentWaypoint;

        if (currentWaypoint == null)
        {
            Debug.LogError("EnemyAI: No se encontró ningún waypoint cercano para " + gameObject.name);
            return;
        }

        GoToNextWaypoint();
    }

    void Update()
    {
        if (player == null || agent == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // ===== FEEDBACK VISUAL =====
        if (rend != null && alertMat != null)
        {
            if (distanceToPlayer <= alertRange)
                rend.material = alertMat;
            else
                rend.material = originalMat;
        }

        // ===== PERSECUCIÓN =====
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

        // ===== ROBO =====
        if (chasing && distanceToPlayer <= robberyDistance)
        {
            RobPlayer();
        }
    }

    void GoToNextWaypoint()
    {
        if (agent == null) return;

        if (currentWaypoint == null)
        {
            Debug.LogError("EnemyAI: currentWaypoint es NULL en " + gameObject.name);
            return;
        }

        if (currentWaypoint.connectedWaypoints == null || currentWaypoint.connectedWaypoints.Length == 0)
        {
            Debug.LogWarning("EnemyAI: No hay waypoints conectados, volviendo al inicial.");
            currentWaypoint = startWaypoint;
        }
        else
        {
            Waypoint nextWaypoint = currentWaypoint.connectedWaypoints[
                Random.Range(0, currentWaypoint.connectedWaypoints.Length)
            ];
            currentWaypoint = nextWaypoint;
        }

        agent.SetDestination(currentWaypoint.transform.position);
    }

    Waypoint FindClosestWaypoint()
    {
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

        if (Menu.Instance != null)
        {
            Menu.Instance.ShowDefeat();
            Menu.Instance.PauseGame();
        }
    }
}