using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace WarriorPlatformer
{
    /// <summary>
    /// Manages the Main Menu UI for "WINTER WARRIOR".
    /// This script programmatically creates the UI elements to match the dark fantasy winter aesthetic.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private string playSceneName = "SampleScene";

        [Header("UI Styling")]
        [SerializeField] private Color titleColor = new Color(0.8f, 0.9f, 1f); // Icy Blue
        [SerializeField] private Color subtitleColor = new Color(0.6f, 0.7f, 0.8f);
        [SerializeField] private Color buttonNormalColor = new Color(0.1f, 0.15f, 0.2f, 0.8f);
        [SerializeField] private Color buttonHighlightColor = new Color(0.3f, 0.5f, 0.7f, 0.9f);

        private Canvas mainCanvas;

        private void Start()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            // 1. Create Canvas
            GameObject canvasObj = new GameObject("MainMenu_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            mainCanvas = canvasObj.GetComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // 2. Create Title (Upper Left)
            CreateTitleGroup(canvasObj.transform);

            // 3. Create Menu Buttons (Lower Right)
            CreateButtonsGroup(canvasObj.transform);

            // 4. Create Settings Icon (Upper Right)
            CreateSettingsIcon(canvasObj.transform);
        }

        private void CreateTitleGroup(Transform parent)
        {
            GameObject group = new GameObject("TitleGroup", typeof(RectTransform));
            group.transform.SetParent(parent, false);
            RectTransform rect = group.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(80, -80);
            rect.sizeDelta = new Vector2(600, 200);

            // Main Title: WINTER WARRIOR
            GameObject titleObj = new GameObject("Title_Text", typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(group.transform, false);
            TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
            titleText.text = "WINTER WARRIOR";
            titleText.fontSize = 84;
            titleText.fontWeight = FontWeight.Bold;
            titleText.color = titleColor;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            
            // Add subtle shadow/outline effect if possible (requires TMP settings)
            titleText.outlineWidth = 0.2f;
            titleText.outlineColor = new Color(0, 0.1f, 0.3f);

            // Subtitle: CHIẾN BINH MÙA ĐÔNG
            GameObject subObj = new GameObject("Subtitle_Text", typeof(TextMeshProUGUI));
            subObj.transform.SetParent(group.transform, false);
            RectTransform subRect = subObj.GetComponent<RectTransform>();
            subRect.anchoredPosition = new Vector2(5, -90);
            
            TextMeshProUGUI subText = subObj.GetComponent<TextMeshProUGUI>();
            subText.text = "CHIẾN BINH MÙA ĐÔNG";
            subText.fontSize = 28;
            subText.color = subtitleColor;
            subText.alignment = TextAlignmentOptions.Left;
        }

        private void CreateButtonsGroup(Transform parent)
        {
            GameObject group = new GameObject("ButtonsGroup", typeof(RectTransform));
            group.transform.SetParent(parent, false);
            RectTransform rect = group.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(1, 0);
            rect.anchoredPosition = new Vector2(-80, 80);
            rect.sizeDelta = new Vector2(300, 300);

            // Vertical Layout for buttons
            VerticalLayoutGroup layout = group.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.LowerRight;
            layout.childControlHeight = false;
            layout.childControlWidth = false;

            // CHƠI (Play)
            CreateMenuButton(group.transform, "CHƠI", 260, 70, true, OnPlayClicked);
            
            // TÙY CHỌN (Options)
            CreateMenuButton(group.transform, "TÙY CHỌN", 200, 50, false, OnOptionsClicked);
            
            // THOÁT GAME (Exit)
            CreateMenuButton(group.transform, "THOÁT GAME", 200, 50, false, OnExitClicked);
        }

        private void CreateMenuButton(Transform parent, string label, float width, float height, bool isPrimary, UnityEngine.Events.UnityAction action)
        {
            GameObject btnObj = new GameObject(label + "_Button", typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, height);

            Image img = btnObj.GetComponent<Image>();
            img.color = isPrimary ? new Color(0.2f, 0.4f, 0.6f, 0.9f) : buttonNormalColor;
            
            // Stylized border (simplified)
            Outline outline = btnObj.AddComponent<Outline>();
            outline.effectColor = isPrimary ? new Color(0.5f, 0.8f, 1f, 0.8f) : new Color(0.3f, 0.3f, 0.3f, 0.5f);
            outline.effectDistance = new Vector2(2, -2);

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(action);

            // Button Text
            GameObject txtObj = new GameObject("Text", typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI txt = txtObj.GetComponent<TextMeshProUGUI>();
            txt.text = label;
            txt.fontSize = isPrimary ? 32 : 20;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Center;
            
            RectTransform txtRect = txtObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;
        }

        private void CreateSettingsIcon(Transform parent)
        {
            GameObject gearObj = new GameObject("Settings_Icon", typeof(Image), typeof(Button));
            gearObj.transform.SetParent(parent, false);
            RectTransform rect = gearObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-40, -40);
            rect.sizeDelta = new Vector2(50, 50);

            Image img = gearObj.GetComponent<Image>();
            img.color = new Color(1, 1, 1, 0.7f);
            // In a real project, we'd assign a gear sprite here.
            
            Button btn = gearObj.GetComponent<Button>();
            btn.onClick.AddListener(OnOptionsClicked);
        }

        // --- Button Callbacks ---

        private void OnPlayClicked()
        {
            Debug.Log("Starting game...");
            SceneManager.LoadScene(playSceneName);
        }

        private void OnOptionsClicked()
        {
            Debug.Log("Opening options...");
        }

        private void OnExitClicked()
        {
            Debug.Log("Exiting game...");
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
