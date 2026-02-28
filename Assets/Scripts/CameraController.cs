using UnityEngine;

/// <summary>
/// Third-person camera with smooth follow, orbit, and sprint zoom.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private PlayerController player;

    [Header("Follow")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 4.5f, -6.5f);
    [SerializeField] private float followSmoothTime = 0.08f;

    [Header("Look")]
    [SerializeField] private float sensitivityX = 120f;
    [SerializeField] private float sensitivityY = 90f;
    [SerializeField] private float minPitch = -20f;
    [SerializeField] private float maxPitch = 60f;

    [Header("Zoom")]
    [SerializeField] private float normalFov = 60f;
    [SerializeField] private float sprintFov = 66f;
    [SerializeField] private float zoomLerpSpeed = 6f;

    private Vector3 currentVelocity;
    private float yaw;
    private float pitch;
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (target == null && player != null)
        {
            target = player.transform;
        }

        Vector3 euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        HandleOrbitInput();
        HandleFollow();
        HandleSprintZoom();
    }

    private void HandleOrbitInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityY * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void HandleFollow()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, followSmoothTime);
        transform.LookAt(target.position + Vector3.up * 1.4f);
    }

    private void HandleSprintZoom()
    {
        if (cam == null || player == null)
        {
            return;
        }

        float targetFov = player.IsSprinting ? sprintFov : normalFov;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, zoomLerpSpeed * Time.deltaTime);
    }
}
