using UnityEngine;

namespace WarriorPlatformer
{
    public class PortalGuidance : MonoBehaviour
    {
        [Header("Guidance Settings")]
        [SerializeField] private Transform targetPortal;
        [SerializeField] private float indicatorDistance = 2f;
        [SerializeField] private float floatSpeed = 2f;
        [SerializeField] private float floatAmplitude = 0.2f;

        [Header("Visual Components")]
        [SerializeField] private GameObject indicatorObject;

        private Transform playerTransform;
        private bool isVisible = false;

        private void Start()
        {
            if (indicatorObject == null)
            {
                Transform visual = transform.Find("Visual");
                if (visual != null) indicatorObject = visual.gameObject;
            }

            if (indicatorObject != null)
            {
                indicatorObject.SetActive(false);
            }

            FindPlayer();

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelCleared += ShowGuidance;
                
                // Check if already cleared
                if (LevelManager.Instance.DefeatedEnemiesCount >= LevelManager.Instance.EnemiesToDefeatGoal)
                {
                    ShowGuidance();
                }
            }
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelCleared -= ShowGuidance;
            }
        }

        private void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        private void ShowGuidance()
        {
            if (isVisible) return;
            
            isVisible = true;
            if (indicatorObject != null)
            {
                indicatorObject.SetActive(true);
            }
            
            Debug.Log("[Guidance] Portal guidance ACTIVATED!");
        }

        private void Update()
        {
            if (!isVisible || targetPortal == null) return;
            
            if (playerTransform == null)
            {
                FindPlayer();
                return;
            }

            // Direction to portal
            Vector3 direction = (targetPortal.position - playerTransform.position).normalized;
            
            // Position indicator in front of player towards portal
            Vector3 targetPosition = playerTransform.position + direction * indicatorDistance;
            
            // Add a floating effect
            targetPosition.y += Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            
            transform.position = targetPosition;
            
            // Rotate to point at portal
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        public void SetTarget(Transform portal)
        {
            targetPortal = portal;
        }
    }
}
