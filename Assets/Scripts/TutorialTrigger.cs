using UnityEngine;
using TMPro;

namespace WarriorPlatformer
{
    public class TutorialTrigger : MonoBehaviour
    {
        [Header("Tutorial Message")]
        [TextArea(2, 4)]
        [SerializeField] private string hintMessage = "Dùng A / D để di chuyển";
        [SerializeField] private TextMeshProUGUI tutorialText;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && tutorialText != null)
            {
                tutorialText.text = hintMessage;
                tutorialText.gameObject.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") && tutorialText != null)
            {
                tutorialText.gameObject.SetActive(false);
            }
        }
    }
}