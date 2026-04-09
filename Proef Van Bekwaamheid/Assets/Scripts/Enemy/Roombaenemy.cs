using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Roombaenemy : MonoBehaviour
{
    [Header("Wander Settings")]
    public float wanderRadius = 10f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    private NavMeshAgent _agent;
    private float _waitTimer;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        SetNewDestination();
    }

    private void Update()
    {
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
                SetNewDestination();
        }
    }

    private void SetNewDestination()
    {
        Vector3 randomPoint = GetRandomNavMeshPoint();
        _agent.SetDestination(randomPoint);
        _waitTimer = Random.Range(minWaitTime, maxWaitTime);
    }

    private Vector3 GetRandomNavMeshPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * wanderRadius;
            randomDir += transform.position;

            if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
                return hit.position;
        }

        return transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}