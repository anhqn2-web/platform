using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WarriorPlatformer
{
    public class BossHealthUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameObject uiContainer;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TextMeshProUGUI bossNameText;

        [Header("Settings")]
        [SerializeField] private string bossName = "Frozen Guardian";

        private EnemyAI bossTarget;

        private void Start()
        {
            if (uiContainer != null)
                uiContainer.SetActive(false);

            if (bossNameText != null)
                bossNameText.text = bossName;
        }

        public void SetupBoss(EnemyAI boss)
        {
            bossTarget = boss;
            if (uiContainer != null)
                uiContainer.SetActive(true);
            
            if (bossNameText != null)
                bossNameText.text = boss.gameObject.name.Replace("(Clone)", "").Trim();

            UpdateUI();
        }

        private void Update()
        {
            if (bossTarget == null) return;

            UpdateUI();

            if (bossTarget.IsDead)
            {
                Invoke("Hide", 2f); // Delay hiding for a bit
            }
        }

        private void UpdateUI()
        {
            if (bossTarget == null || healthSlider == null) return;

            healthSlider.maxValue = bossTarget.MaxHealth;
            healthSlider.value = bossTarget.CurrentHealth;
        }

        public void Hide()
        {
            if (uiContainer != null)
                uiContainer.SetActive(false);
            bossTarget = null;
        }
    }
}
