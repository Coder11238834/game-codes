using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles match flow: timer, score, goals, and resets.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Match")]
    [SerializeField] private float matchDurationSeconds = 300f; // 5 minutes
    [SerializeField] private BallController ball;
    [SerializeField] private Transform ballStartPoint;
    [SerializeField] private Transform playerStartPoint;
    [SerializeField] private Transform aiStartPoint;

    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private AIController ai;

    public int PlayerScore { get; private set; }
    public int OpponentScore { get; private set; }
    public float RemainingTime { get; private set; }
    public bool MatchEnded { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RemainingTime = matchDurationSeconds;
    }

    private void Update()
    {
        if (MatchEnded)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            return;
        }

        RemainingTime -= Time.deltaTime;
        if (RemainingTime <= 0f)
        {
            RemainingTime = 0f;
            MatchEnded = true;
            Time.timeScale = 0f;
        }
    }

    /// <summary>
    /// Call this when the ball enters a goal trigger.
    /// scoringTeam: "Player" or "Opponent"
    /// </summary>
    public void RegisterGoal(string scoringTeam)
    {
        if (MatchEnded)
        {
            return;
        }

        if (scoringTeam == "Player")
        {
            PlayerScore++;
        }
        else
        {
            OpponentScore++;
        }

        ResetAfterGoal();
    }

    private void ResetAfterGoal()
    {
        Time.timeScale = 1f;

        CharacterController playerController = player.GetComponent<CharacterController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        player.transform.SetPositionAndRotation(playerStartPoint.position, playerStartPoint.rotation);
        ai.transform.SetPositionAndRotation(aiStartPoint.position, aiStartPoint.rotation);

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        ball.ReleaseOwner();
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        ball.transform.SetPositionAndRotation(ballStartPoint.position, ballStartPoint.rotation);
    }
}

/// <summary>
/// Attach this to each goal trigger collider.
/// </summary>
public class GoalTrigger : MonoBehaviour
{
    [SerializeField] private string scoringTeamOnEnter = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BallController>() != null)
        {
            GameManager.Instance.RegisterGoal(scoringTeamOnEnter);
        }
    }
}
