using UnityEngine;
using UnityEngine.SceneManagement;

namespace WarriorPlatformer
{
    public class ChangeScene : MonoBehaviour
    {
        [SerializeField] private string targetSceneName = "SampleScene";

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                SceneManager.LoadScene(targetSceneName);
            }
        }
    }
}