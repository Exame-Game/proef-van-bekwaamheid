using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Roombaenemy : MonoBehaviour
{
    [Header("Wander Settings")]
    public float wanderRadius = 10f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    private NavMeshAgent agent;
    private float waitTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        SetNewDestination();
    }

    void Update()
    {
        // If close enough to destination (or stuck), wait then pick a new one
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
                SetNewDestination();
        }
    }

    void SetNewDestination()
    {
        Vector3 randomPoint = GetRandomNavMeshPoint();
        agent.SetDestination(randomPoint);
        waitTimer = Random.Range(minWaitTime, maxWaitTime);
    }

    Vector3 GetRandomNavMeshPoint()
    {
        // Try up to 10 times to find a valid NavMesh point
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * wanderRadius;
            randomDir += transform.position;

            if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
                return hit.position;
        }

        return transform.position; // Fallback: stay put
    }

    // Draw wander radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}