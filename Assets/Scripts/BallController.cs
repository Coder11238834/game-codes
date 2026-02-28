using UnityEngine;

/// <summary>
/// Controls soccer ball physics and ownership behavior.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private float mass = 0.43f;
    [SerializeField] private float drag = 0.12f;
    [SerializeField] private float angularDrag = 0.08f;
    [SerializeField] private float groundFriction = 0.96f;
    [SerializeField] private float ownerFollowSpeed = 20f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 0.35f;

    private Rigidbody rb;
    private MonoBehaviour owner;
    private Transform ownerHoldPoint;

    public bool IsOwned => owner != null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.drag = drag;
        rb.angularDrag = angularDrag;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        if (IsOwned && ownerHoldPoint != null)
        {
            FollowOwner();
            return;
        }

        SimulateGroundFriction();
    }

    public void SetOwner(MonoBehaviour newOwner, Transform holdPoint)
    {
        owner = newOwner;
        ownerHoldPoint = holdPoint;

        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public bool IsOwnedBy(MonoBehaviour possibleOwner)
    {
        return owner == possibleOwner;
    }

    public void Kick(Vector3 direction, float force, float releaseCooldown)
    {
        ReleaseOwner();
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);

        // Prevent immediate re-capture by the same frame overlap.
        if (releaseCooldown > 0f)
        {
            StartCoroutine(CaptureCooldown(releaseCooldown));
        }
    }

    public void ReleaseOwner()
    {
        owner = null;
        ownerHoldPoint = null;
        rb.isKinematic = false;
    }

    private void FollowOwner()
    {
        Vector3 targetPosition = ownerHoldPoint.position;
        Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, ownerFollowSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);
    }

    private void SimulateGroundFriction()
    {
        if (Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask))
        {
            rb.velocity = new Vector3(rb.velocity.x * groundFriction, rb.velocity.y, rb.velocity.z * groundFriction);
        }
    }

    private System.Collections.IEnumerator CaptureCooldown(float cooldown)
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        yield return new WaitForSeconds(cooldown);

        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }
    }
}
