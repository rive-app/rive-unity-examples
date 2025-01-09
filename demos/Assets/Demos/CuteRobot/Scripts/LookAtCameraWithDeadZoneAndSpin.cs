using UnityEngine;
using System.Collections;

namespace Demos.CuteRobot
{
    /// <summary>
    /// Controls the robot's head tracking behavior with a camera, including periodic playful spins.
    /// Features a dead zone to prevent minor jittering and smooth transitions between states.
    /// </summary>
    [RequireComponent(typeof(Transform))]
    public class LookAtCameraWithDeadZoneAndSpin : MonoBehaviour
    {
        [Header("Look At Settings")]
        [SerializeField, Range(0.01f, 4f), Tooltip("Speed at which the robot rotates towards the camera")]
        private float rotationSpeed = 0.1f;

        [SerializeField, Range(0f, 45f), Tooltip("Angle in degrees within which the robot won't adjust its rotation")]
        private float deadZoneAngle = 5f;

        [Header("Spin Settings")]
        [SerializeField, Range(1f, 30f), Tooltip("Time between automatic spins (in seconds)")]
        private float spinFrequency = 5f;

        [SerializeField, Range(50f, 1000f), Tooltip("Speed of the playful spin animation")]
        private float spinSpeed = 200f;

        [SerializeField, Range(0f, 90f), Tooltip("Additional degrees to rotate past 360 for playful effect")]
        private float overshootDegrees = 45f;

        [SerializeField, Range(0.1f, 1f), Tooltip("Time taken to settle back after overshooting")]
        private float settleTime = 0.2f;

        [Header("References")]
        [SerializeField, Tooltip("Camera to look at. Will use Main Camera if not set")]
        private Transform cameraTransform;

        private float targetSpinDegrees;
        private float timeSinceLastSpin;
        private bool isSpinning;
        private float spinProgress;
        private Coroutine settleCoroutine;

        private void OnValidate()
        {
            // Ensure spin frequency is greater than settle time
            spinFrequency = Mathf.Max(spinFrequency, settleTime + 0.5f);
        }

        private void Start()
        {
            InitializeCamera();
        }

        private void OnEnable()
        {
            ResetSpinState();
        }

        private void OnDisable()
        {
            if (settleCoroutine != null)
            {
                StopCoroutine(settleCoroutine);
                settleCoroutine = null;
            }
        }

        private void Update()
        {
            if (!ValidateCamera()) return;

            timeSinceLastSpin += Time.deltaTime;

            if (ShouldStartSpin())
            {
                StartSpin();
            }

            if (isSpinning)
            {
                UpdateSpin();
            }
            else
            {
                UpdateLookAt();
            }
        }

        private void InitializeCamera()
        {
            if (cameraTransform == null)
            {
                var mainCam = Camera.main;
                if (mainCam != null)
                {
                    cameraTransform = mainCam.transform;
                }
                else
                {
                    Debug.LogWarning($"No camera assigned to {gameObject.name} and no Main Camera found in scene.");
                }
            }
        }

        private bool ValidateCamera()
        {
            if (cameraTransform == null)
            {
                InitializeCamera();
                return cameraTransform != null;
            }
            return true;
        }

        private bool ShouldStartSpin()
        {
            return timeSinceLastSpin >= spinFrequency && !isSpinning;
        }

        private void StartSpin()
        {
            isSpinning = true;
            spinProgress = 0f;
            targetSpinDegrees = 360f + overshootDegrees;
        }

        private void ResetSpinState()
        {
            isSpinning = false;
            spinProgress = 0f;
            timeSinceLastSpin = 0f;
        }

        private void UpdateSpin()
        {
            float deltaRotation = spinSpeed * Time.deltaTime;
            spinProgress += deltaRotation;
            transform.Rotate(0, deltaRotation, 0, Space.Self);

            if (spinProgress >= targetSpinDegrees)
            {
                CompleteSpin();
            }
        }

        private void CompleteSpin()
        {
            isSpinning = false;
            timeSinceLastSpin = 0;

            if (settleCoroutine != null)
            {
                StopCoroutine(settleCoroutine);
            }
            settleCoroutine = StartCoroutine(SettleRotation());
        }

        private void UpdateLookAt()
        {
            Vector3 targetDirection = cameraTransform.position - transform.position;
            float angleToCamera = Vector3.Angle(transform.forward, targetDirection);

            if (angleToCamera > deadZoneAngle)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        private IEnumerator SettleRotation()
        {
            Quaternion finalRotation = Quaternion.LookRotation(cameraTransform.position - transform.position);
            Quaternion overshootRotation = transform.rotation;
            float elapsedTime = 0f;

            while (elapsedTime < settleTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / settleTime;

                // Use smoothstep for more natural easing
                float smoothT = t * t * (3f - 2f * t);

                transform.rotation = Quaternion.Lerp(overshootRotation, finalRotation, smoothT);
                yield return null;
            }

            transform.rotation = finalRotation;
            settleCoroutine = null;
        }

        /// <summary>
        /// Forces the robot to immediately look at the camera, canceling any ongoing spin.
        /// </summary>
        public void ForceLookAtCamera()
        {
            if (!ValidateCamera()) return;

            if (settleCoroutine != null)
            {
                StopCoroutine(settleCoroutine);
                settleCoroutine = null;
            }

            ResetSpinState();
            Vector3 targetDirection = cameraTransform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(targetDirection);
        }

        /// <summary>
        /// Sets a new camera transform to look at.
        /// </summary>
        /// <param name="newCamera">The transform of the new camera to look at</param>
        public void SetCamera(Transform newCamera)
        {
            if (newCamera == null)
            {
                Debug.LogWarning($"Attempted to set null camera on {gameObject.name}");
                return;
            }

            cameraTransform = newCamera;
            ForceLookAtCamera();
        }
    }
}