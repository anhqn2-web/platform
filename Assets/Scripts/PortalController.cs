using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace WarriorPlatformer
{
    public class PortalController : MonoBehaviour
    {
        [Header("Portal Settings")]
        [SerializeField] private string nextSceneName = "SampleScene";
        [SerializeField] private float activationDelay = 1f;
        [SerializeField] private Color lockedColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color unlockedColor = new Color(0f, 0.8f, 1f, 1f);

        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer portalRenderer;
        [SerializeField] private ParticleSystem portalParticles;
        [SerializeField] private Light portalLight;

        private bool isUnlocked = false;
        private bool isTransitioning = false;

        private void Start()
        {
            // Initial state: Locked
            UpdatePortalVisuals(false);

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelCleared += UnlockPortal;
                
                // Check if already cleared (e.g. if script added late)
                if (LevelManager.Instance.DefeatedEnemiesCount >= LevelManager.Instance.EnemiesToDefeatGoal)
                {
                    UnlockPortal();
                }
            }
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelCleared -= UnlockPortal;
            }
        }

        private void UnlockPortal()
        {
            if (isUnlocked) return;
            
            StartCoroutine(UnlockSequence());
        }

        private IEnumerator UnlockSequence()
        {
            yield return new WaitForSeconds(activationDelay);
            
            isUnlocked = true;
            UpdatePortalVisuals(true);
            
            Debug.Log("[Portal] Portal has been UNLOCKED!");
            
            // Optional: Play sound effect here
        }

        private void UpdatePortalVisuals(bool unlocked)
        {
            if (portalRenderer != null)
            {
                portalRenderer.color = unlocked ? unlockedColor : lockedColor;
            }

            if (portalParticles != null)
            {
                if (unlocked) portalParticles.Play();
                else portalParticles.Stop();
            }

            if (portalLight != null)
            {
                portalLight.enabled = unlocked;
                portalLight.color = unlockedColor;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isUnlocked || isTransitioning) return;

            if (other.CompareTag("Player"))
            {
                EnterPortal();
            }
        }

        private void EnterPortal()
        {
            isTransitioning = true;
            Debug.Log("[Portal] Player entering portal. Loading: " + nextSceneName);
            
            // Play entry effect if any
            
            StartCoroutine(LoadNextLevelRoutine());
        }

        private IEnumerator LoadNextLevelRoutine()
        {
            // Small delay for effect
            yield return new WaitForSeconds(0.5f);
            
            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogError($"[Portal] Cannot load scene '{nextSceneName}'. Make sure it's in Build Settings.");
                isTransitioning = false;
            }
        }
        
        // Helper to setup components programmatically if needed
        public void SetupComponents(SpriteRenderer sr, ParticleSystem ps, Light l)
        {
            portalRenderer = sr;
            portalParticles = ps;
            portalLight = l;
        }
    }
}
