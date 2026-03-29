using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private InputService inputService;

    [Header("Orbit")]
    [SerializeField] private float distance = 6f;
    [SerializeField] private float height = 1.6f;
    [SerializeField] private float lookSensitivity = 120f;
    [SerializeField] private float minPitch = -25f;
    [SerializeField] private float maxPitch = 65f;

    private float yaw;
    private float pitch = 15f;

    private void Awake()
    {
        if (inputService == null)
        {
            inputService = FindFirstObjectByType<InputService>();
        }

        Vector3 euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;
    }

    private void LateUpdate()
    {
        if (target == null || inputService == null)
        {
            return;
        }

        Vector2 lookInput = inputService.Look;
        yaw += lookInput.x * lookSensitivity * Time.deltaTime;
        pitch -= lookInput.y * lookSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focusPoint = target.position + Vector3.up * height;
        Vector3 cameraPosition = focusPoint - rotation * Vector3.forward * distance;

        transform.SetPositionAndRotation(cameraPosition, rotation);
    }
}
