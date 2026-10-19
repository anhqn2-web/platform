using UnityEngine;
using TMPro;

namespace WarriorPlatformer
{
    public class EnemyCounterUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI counterText;

        private void Awake()
        {
            // Tự động lấy TextMeshPro trên cùng GameObject nếu chưa kéo thả
            if (counterText == null)
            {
                counterText = GetComponent<TextMeshProUGUI>();
            }
        }

        private void Start()
        {
            if (LevelManager.Instance != null)
            {
                // Đăng ký nhận sự kiện từ LevelManager
                LevelManager.Instance.OnProgressUpdated += UpdateCounterText;
                
                // Hiển thị chỉ số ban đầu
                UpdateCounterText(LevelManager.Instance.DefeatedEnemiesCount, LevelManager.Instance.EnemiesToDefeatGoal);
            }
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnProgressUpdated -= UpdateCounterText;
            }
        }

        private void UpdateCounterText(int current, int goal)
        {
            if (counterText != null)

            {
                counterText.text = $"Nhiệm vụ: \nTiêu diệt {goal} kẻ địch({current}/{goal})";
            }
        }
    }
}