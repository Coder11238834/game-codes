using UnityEngine;

/// <summary>
/// Handles local player movement, possession, passing, and shooting.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float gravity = -20f;

    [Header("Ball Interaction")]
    [SerializeField] private BallController ball;
    [SerializeField] private Transform ballHoldPoint;
    [SerializeField] private float possessionRadius = 1.3f;
    [SerializeField] private float passForce = 10f;
    [SerializeField] private float shootForce = 18f;
    [SerializeField] private float shootUpwardForce = 2f;

    private CharacterController characterController;
    private Camera mainCamera;
    private Vector3 velocity;
    private Vector3 moveDirection;

    public bool IsSprinting { get; private set; }
    public bool HasPossession { get; private set; }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;

        if (ball == null)
        {
            ball = FindFirstObjectByType<BallController>();
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleBallPossession();
        HandleBallActions();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(horizontal, 0f, vertical).normalized;
        IsSprinting = Input.GetKey(KeyCode.LeftShift);

        if (input.sqrMagnitude > 0.01f)
        {
            // Camera-relative movement keeps controls intuitive in third-person.
            Vector3 cameraForward = mainCamera.transform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            Vector3 cameraRight = mainCamera.transform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            moveDirection = (cameraForward * input.z + cameraRight * input.x).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        float currentSpeed = IsSprinting ? sprintSpeed : moveSpeed;
        Vector3 planarMove = moveDirection * currentSpeed;

        if (characterController.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;

        characterController.Move((planarMove + new Vector3(0f, velocity.y, 0f)) * Time.deltaTime);
    }

    private void HandleBallPossession()
    {
        if (ball == null)
        {
            HasPossession = false;
            return;
        }

        float distanceToBall = Vector3.Distance(transform.position, ball.transform.position);

        // Acquire possession if close and no one else owns the ball.
        if (!HasPossession && distanceToBall <= possessionRadius && !ball.IsOwned)
        {
            HasPossession = true;
            ball.SetOwner(this, ballHoldPoint);
        }

        // Drop possession if the ball was detached by an external event.
        if (HasPossession && !ball.IsOwnedBy(this))
        {
            HasPossession = false;
        }
    }

    private void HandleBallActions()
    {
        if (!HasPossession || ball == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Vector3 passDirection = transform.forward;
            ball.Kick(passDirection, passForce, 0.8f);
            HasPossession = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 shootDirection = (transform.forward + Vector3.up * (shootUpwardForce / shootForce)).normalized;
            ball.Kick(shootDirection, shootForce, 1.2f);
            HasPossession = false;
        }
    }
}
