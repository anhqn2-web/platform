using UnityEngine;

namespace WarriorPlatformer
{
    public class PuzzleCrystal : MonoBehaviour
    {
        [Header("Crystal Settings")]
        [SerializeField] private PuzzleMechanism mechanism;
        [SerializeField] private Color inactiveColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color activeColor = new Color(0f, 1f, 1f, 1f);

        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer crystalRenderer;
        [SerializeField] private ParticleSystem glowParticles;

        private void Start()
        {
            if (crystalRenderer == null) crystalRenderer = GetComponent<SpriteRenderer>();
            if (glowParticles == null) glowParticles = GetComponentInChildren<ParticleSystem>();

            if (mechanism != null)
            {
                mechanism.OnStateChanged += UpdateVisuals;
                UpdateVisuals(mechanism.IsActivated);
            }
        }

        private void OnDestroy()
        {
            if (mechanism != null)
            {
                mechanism.OnStateChanged -= UpdateVisuals;
            }
        }

        private void UpdateVisuals(bool isActive)
        {
            if (crystalRenderer != null)
            {
                crystalRenderer.color = isActive ? activeColor : inactiveColor;
            }

            if (glowParticles != null)
            {
                if (isActive) glowParticles.Play();
                else glowParticles.Stop();
            }
        }
    }
}
