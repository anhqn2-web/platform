using UnityEngine;
using UnityEngine.UI;

namespace WarriorPlatformer
{
    /// <summary>
    /// Manages the Player's HUD (Health and Stamina bars).
    /// This script is designed to be plug-and-play: it will automatically find the player
    /// and can even generate its own UI if none is provided in the inspector.
    /// </summary>
    public class PlayerUI : MonoBehaviour
    {
        [Header("Player Reference")]
        [SerializeField] private WarriorController player;

        [Header("UI Elements")]
        [SerializeField] private Image healthBarFill;
        [SerializeField] private Image staminaBarFill;

        [Header("Colors")]
        [SerializeField] private Color healthColor = new Color(0.85f, 0.15f, 0.15f);
        [SerializeField] private Color staminaColor = new Color(0.15f, 0.65f, 0.85f);

        private Text counterText;
        private Text questInfoText;
        
        // Skill Panel Elements
        private Image skillIconFill;
        private Text skillCooldownText;

        private void Start()
        {
            // Auto-detect player if not assigned
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) player = playerObj.GetComponent<WarriorController>();
            }

            // If UI elements are missing, create a basic stylized HUD programmatically
            if (healthBarFill == null || staminaBarFill == null)
            {
                CreateDefaultHUD();
            }

            if (LevelManager.Instance != null)
            {
                UpdateCounter(LevelManager.Instance.DefeatedEnemiesCount, LevelManager.Instance.EnemiesToDefeatGoal);
                LevelManager.Instance.OnProgressUpdated += UpdateCounter;
                LevelManager.Instance.OnLevelCleared += HandleLevelCleared;
            }
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnProgressUpdated -= UpdateCounter;
                LevelManager.Instance.OnLevelCleared -= HandleLevelCleared;
            }
        }

        private void UpdateCounter(int current, int goal)
        {
            if (counterText != null)
            {
                counterText.text = $"Enemies: {current}/{goal}";
                
                // Add a progress bar feel to the text
                string progressBar = "[";
                int barLength = 10;
                int filled = Mathf.RoundToInt((float)current / goal * barLength);
                for (int i = 0; i < barLength; i++)
                {
                    progressBar += i < filled ? "=" : " ";
                }
                progressBar += "]";
                
                counterText.text = $"Quest: {LevelManager.Instance?.QuestTitle}\nProgress: {current}/{goal} {progressBar}";
            }
        }

        private void HandleLevelCleared()
        {
            if (counterText != null)
            {
                counterText.text = "LEVEL CLEARED!";
                counterText.color = Color.yellow;
            }
        }

        private void Update()
        {
            if (player == null) return;

            // Update fill amounts smoothly based on current player stats
            if (healthBarFill != null)
            {
                float targetHealth = player.CurrentHealth / player.MaxHealth;
                healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetHealth, Time.deltaTime * 10f);
            }

            if (staminaBarFill != null)
            {
                float targetStamina = player.CurrentStamina / player.MaxStamina;
                staminaBarFill.fillAmount = Mathf.Lerp(staminaBarFill.fillAmount, targetStamina, Time.deltaTime * 10f);
            }

            UpdateSkillPanel();
        }

        private void UpdateSkillPanel()
        {
            if (player == null || skillIconFill == null) return;

            float cooldownRatio = player.HealCooldownRatio;
            float remaining = player.HealCooldownRemaining;

            // Fill amount represents the "overlay" mask for cooldown
            skillIconFill.fillAmount = cooldownRatio;

            if (skillCooldownText != null)
            {
                if (remaining > 0)
                {
                    skillCooldownText.text = remaining.ToString("F1") + "s";
                    skillIconFill.color = new Color(0, 0, 0, 0.6f);
                }
                else
                {
                    skillCooldownText.text = "READY [Q]";
                    skillIconFill.color = new Color(0, 1, 0, 0.1f); // Subtle green tint when ready
                }
            }
        }

        private void CreateDefaultHUD()
        {
            // 1. Create Canvas
            GameObject canvasObj = new GameObject("PlayerHUD_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            // 2. Create Container for bars
            GameObject container = new GameObject("BarsContainer", typeof(RectTransform));
            container.transform.SetParent(canvasObj.transform, false);
            RectTransform containerRect = container.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 1);
            containerRect.anchorMax = new Vector2(0, 1);
            containerRect.pivot = new Vector2(0, 1);
            containerRect.anchoredPosition = new Vector2(20, -20);
            containerRect.sizeDelta = new Vector2(200, 60);

            // 3. Create Health Bar
            healthBarFill = CreateBar(container.transform, "HealthBar", 0, healthColor);
            
            // 4. Create Stamina Bar
            staminaBarFill = CreateBar(container.transform, "StaminaBar", -25, staminaColor);

            // 5. Create Counter Text
            GameObject textObj = new GameObject("EnemyCounter", typeof(Text));
            textObj.transform.SetParent(canvasObj.transform, false);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(1, 1);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.pivot = new Vector2(1, 1);
            textRect.anchoredPosition = new Vector2(-20, -20);
            textRect.sizeDelta = new Vector2(350, 80); // Increased size for multiline

            counterText = textObj.GetComponent<Text>();
            counterText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            counterText.fontSize = 20; // Slightly smaller to fit better
            counterText.fontStyle = FontStyle.Bold;
            counterText.alignment = TextAnchor.UpperRight;
            counterText.color = Color.white;
            counterText.text = "Enemies: 0/20";

            // 6. Create Quest Description Text
            GameObject descObj = new GameObject("QuestDescription", typeof(Text));
            descObj.transform.SetParent(canvasObj.transform, false);
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(1, 1);
            descRect.anchorMax = new Vector2(1, 1);
            descRect.pivot = new Vector2(1, 1);
            descRect.anchoredPosition = new Vector2(-20, -100); // Moved further down
            descRect.sizeDelta = new Vector2(300, 60);

            questInfoText = descObj.GetComponent<Text>();
            questInfoText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            questInfoText.fontSize = 14;
            questInfoText.fontStyle = FontStyle.Italic;
            questInfoText.alignment = TextAnchor.UpperRight;
            questInfoText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            
            if (LevelManager.Instance != null)
            {
                questInfoText.text = LevelManager.Instance.QuestDescription;
            }

            CreateSkillPanel(canvasObj.transform);
        }

        private void CreateSkillPanel(Transform parent)
        {
            // Skill Panel Container (Bottom Right)
            GameObject panelObj = new GameObject("SkillPanel", typeof(RectTransform));
            panelObj.transform.SetParent(parent, false);
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.pivot = new Vector2(1, 0);
            panelRect.anchoredPosition = new Vector2(-20, 20);
            panelRect.sizeDelta = new Vector2(100, 100);

            // Skill Icon Background
            GameObject iconBGObj = new GameObject("HealIcon_BG", typeof(Image));
            iconBGObj.transform.SetParent(panelRect, false);
            RectTransform iconBGRect = iconBGObj.GetComponent<RectTransform>();
            iconBGRect.sizeDelta = new Vector2(64, 64);
            iconBGRect.anchoredPosition = new Vector2(0, 0);
            iconBGObj.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Skill Icon Fill (Cooldown Overlay)
            GameObject iconFillObj = new GameObject("HealIcon_Fill", typeof(Image));
            iconFillObj.transform.SetParent(iconBGRect, false);
            RectTransform iconFillRect = iconFillObj.GetComponent<RectTransform>();
            iconFillRect.anchorMin = Vector2.zero;
            iconFillRect.anchorMax = Vector2.one;
            iconFillRect.sizeDelta = Vector2.zero;
            
            skillIconFill = iconFillObj.GetComponent<Image>();
            skillIconFill.type = Image.Type.Filled;
            skillIconFill.fillMethod = Image.FillMethod.Radial360;
            skillIconFill.fillOrigin = (int)Image.Origin360.Top;
            skillIconFill.color = new Color(0, 0, 0, 0.6f);

            // Skill Label (Q)
            GameObject labelObj = new GameObject("SkillLabel", typeof(Text));
            labelObj.transform.SetParent(panelRect, false);
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchoredPosition = new Vector2(0, -40);
            labelRect.sizeDelta = new Vector2(100, 20);
            
            skillCooldownText = labelObj.GetComponent<Text>();
            skillCooldownText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            skillCooldownText.fontSize = 16;
            skillCooldownText.alignment = TextAnchor.MiddleCenter;
            skillCooldownText.color = Color.white;
            skillCooldownText.text = "READY [Q]";
            
            // Icon Label "Heal"
            GameObject healTextObj = new GameObject("HealText", typeof(Text));
            healTextObj.transform.SetParent(iconBGRect, false);
            RectTransform healTextRect = healTextObj.GetComponent<RectTransform>();
            healTextRect.sizeDelta = new Vector2(64, 64);
            Text healText = healTextObj.GetComponent<Text>();
            healText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            healText.text = "+HP";
            healText.fontSize = 18;
            healText.fontStyle = FontStyle.Bold;
            healText.alignment = TextAnchor.MiddleCenter;
            healText.color = Color.green;
        }

        private Image CreateBar(Transform parent, string name, float yOffset, Color color)
        {
            // BG
            GameObject bgObj = new GameObject(name + "_BG", typeof(Image));
            bgObj.transform.SetParent(parent, false);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 1);
            bgRect.anchorMax = new Vector2(0, 1);
            bgRect.pivot = new Vector2(0, 1);
            bgRect.anchoredPosition = new Vector2(0, yOffset);
            bgRect.sizeDelta = new Vector2(200, 15);
            bgObj.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);

            // Fill
            GameObject fillObj = new GameObject(name + "_Fill", typeof(Image));
            fillObj.transform.SetParent(bgObj.transform, false);
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            
            Image fillImage = fillObj.GetComponent<Image>();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.color = color;

            return fillImage;
        }
    }
}
