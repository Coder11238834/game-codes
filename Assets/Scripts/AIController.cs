using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Basic AI using state-driven behavior for soccer actions.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AIController : MonoBehaviour
{
    private enum AIState
    {
        Idle,
        ChaseBall,
        Defend,
        Attack
    }

    [Header("References")]
    [SerializeField] private BallController ball;
    [SerializeField] private Transform ownGoal;
    [SerializeField] private Transform opponentGoal;
    [SerializeField] private Transform ballHoldPoint;

    [Header("Behavior")]
    [SerializeField] private float possessionRange = 1.4f;
    [SerializeField] private float defendDistanceFromGoal = 12f;
    [SerializeField] private float shootDistance = 16f;
    [SerializeField] private float shootForce = 16f;
    [SerializeField] private float interceptionLead = 0.4f;
    [SerializeField] private float decisionRate = 0.15f;

    private NavMeshAgent agent;
    private AIState currentState;
    private float decisionTimer;
    private bool hasPossession;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;
        agent.autoBraking = false;

        if (ball == null)
        {
            ball = FindFirstObjectByType<BallController>();
        }
    }

    private void Update()
    {
        if (ball == null)
        {
            return;
        }

        decisionTimer -= Time.deltaTime;
        if (decisionTimer <= 0f)
        {
            decisionTimer = decisionRate;
            EvaluateState();
        }

        ExecuteState();
    }

    private void EvaluateState()
    {
        hasPossession = ball.IsOwnedBy(this);

        if (hasPossession)
        {
            currentState = AIState.Attack;
            return;
        }

        float ballDistance = Vector3.Distance(transform.position, ball.transform.position);
        float goalDistance = Vector3.Distance(transform.position, ownGoal.position);

        if (ballDistance < 9f)
        {
            currentState = AIState.ChaseBall;
        }
        else if (goalDistance > defendDistanceFromGoal)
        {
            currentState = AIState.Defend;
        }
        else
        {
            currentState = AIState.Idle;
        }
    }

    private void ExecuteState()
    {
        switch (currentState)
        {
            case AIState.Idle:
                agent.isStopped = true;
                break;

            case AIState.ChaseBall:
                agent.isStopped = false;
                Vector3 interceptionPoint = GetBallInterceptionPoint();
                agent.SetDestination(interceptionPoint);

                float distanceToBall = Vector3.Distance(transform.position, ball.transform.position);
                if (distanceToBall <= possessionRange && !ball.IsOwned)
                {
                    ball.SetOwner(this, ballHoldPoint);
                    hasPossession = true;
                    currentState = AIState.Attack;
                }
                break;

            case AIState.Defend:
                agent.isStopped = false;
                Vector3 defendPoint = ownGoal.position + (opponentGoal.position - ownGoal.position).normalized * 7f;
                agent.SetDestination(defendPoint);
                break;

            case AIState.Attack:
                agent.isStopped = false;
                agent.SetDestination(opponentGoal.position);

                float distanceToGoal = Vector3.Distance(transform.position, opponentGoal.position);
                if (distanceToGoal <= shootDistance && hasPossession)
                {
                    ShootAtGoal();
                }
                break;
        }
    }

    private Vector3 GetBallInterceptionPoint()
    {
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        if (ballRb == null)
        {
            return ball.transform.position;
        }

        Vector3 predicted = ball.transform.position + ballRb.velocity * interceptionLead;
        return predicted;
    }

    private void ShootAtGoal()
    {
        Vector3 shotDirection = (opponentGoal.position - ball.transform.position).normalized;
        Vector3 liftedDirection = (shotDirection + Vector3.up * 0.1f).normalized;
        ball.Kick(liftedDirection, shootForce, 1f);

        hasPossession = false;
        currentState = AIState.ChaseBall;
    }
}
