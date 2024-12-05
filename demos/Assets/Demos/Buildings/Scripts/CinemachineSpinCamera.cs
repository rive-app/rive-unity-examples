using UnityEngine;
using Cinemachine;

/// <summary>
/// Controls orbital camera movement around a target using Cinemachine.
/// Provides smooth transition from initial speed to a settled speed.
/// </summary>
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CinemachineSpinCamera : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField, Tooltip("Initial rotation speed in degrees per second")]
    private float initialSpeed = 5.0f;

    [SerializeField, Tooltip("Final settled rotation speed in degrees per second")]
    private float settledSpeed = 1.0f;

    [SerializeField, Tooltip("How quickly the camera transitions to settled speed (higher = faster)")]
    private float transitionSpeed = 1.5f;

    [SerializeField]
    private Transform target;

    private float currentSpeed;

    private void Awake()
    {
        currentSpeed = initialSpeed;
    }

    private void Start()
    {
        CinemachineVirtualCamera vcam = GetComponentInChildren<CinemachineVirtualCamera>();
        if (vcam != null && target != null)
        {
            vcam.Follow = target;
            vcam.LookAt = target;
        }
    }

    private void Update()
    {
        // Smoothly transition current speed to the settled speed
        currentSpeed = Mathf.Lerp(currentSpeed, settledSpeed,
            Mathf.Min(1.0f, transitionSpeed * Time.deltaTime));


        if (target != null)
        {
            // Rotate around target's position about the Y-axis
            transform.RotateAround(target.position, Vector3.up, currentSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Resets the rotation speed to the initial speed.
    /// </summary>
    public void ResetSpeed()
    {
        currentSpeed = initialSpeed;
    }

    /// <summary>
    /// Set a new target to orbit around.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        CinemachineVirtualCamera vcam = GetComponentInChildren<CinemachineVirtualCamera>();
        if (vcam != null && target != null)
        {
            vcam.Follow = target;
            vcam.LookAt = target;
        }
    }
}
