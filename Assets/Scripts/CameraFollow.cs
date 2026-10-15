using UnityEngine;

namespace WarriorPlatformer
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target Settings")]
        [Tooltip("The target the camera should follow. If left empty, it will automatically find the GameObject tagged 'Player'.")]
        [SerializeField] private Transform target;

        [Header("Follow Settings")]
        [Tooltip("Offset of the camera relative to the target.")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -10f);
        [Tooltip("Smoothing time. Lower values are faster, higher values are smoother.")]
        [SerializeField] private float smoothTime = 0.25f;

        // Velocity reference used by Vector3.SmoothDamp
        private Vector3 currentVelocity = Vector3.zero;

        // Screen Shake variables
        private float shakeDurationRemaining = 0f;
        private float shakeMagnitude = 0f;
        private Vector3 shakeOffset = Vector3.zero;

        private void Start()
        {
            // If no target is assigned in the inspector, auto-detect the Player
            if (target == null)
            {
                FindPlayerTarget();
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                FindPlayerTarget();
                if (target == null) return; // If player still not found, wait
            }

            // Calculate target position with offset
            Vector3 targetPosition = target.position + offset;

            // Smoothly interpolate camera position using SmoothDamp
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

            // Handle Camera Shake
            if (shakeDurationRemaining > 0)
            {
                // Generate a random vector offset
                shakeOffset = Random.insideUnitSphere * shakeMagnitude;
                
                // Keep the Z offset unaffected so we don't shake the camera through the clipping planes
                shakeOffset.z = 0f;

                // Subtract remaining time
                shakeDurationRemaining -= Time.deltaTime;
            }
            else
            {
                shakeOffset = Vector3.zero;
            }

            // Apply final position + shake offset
            transform.position = smoothedPosition + shakeOffset;
        }

        private void FindPlayerTarget()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        /// <summary>
        /// Triggers a screen shake effect (e.g. during combat or when taking damage).
        /// </summary>
        /// <param name="duration">Duration of the shake in seconds.</param>
        /// <param name="magnitude">Intensity of the shake.</param>
        public void TriggerShake(float duration, float magnitude)
        {
            shakeDurationRemaining = duration;
            shakeMagnitude = magnitude;
        }
    }
}
