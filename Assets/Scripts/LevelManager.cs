using UnityEngine;

namespace WarriorPlatformer
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Level Settings")]
        [SerializeField] private string questTitle = "Clearing the Path";
        [SerializeField] private string questDescription = "Defeat the enemies blocking your progress.";
        [SerializeField] private int enemiesToDefeatGoal = 20;
        [SerializeField] private int defeatedEnemiesCount = 0;

        public string QuestTitle => questTitle;
        public string QuestDescription => questDescription;
        public int DefeatedEnemiesCount => defeatedEnemiesCount;
        public int EnemiesToDefeatGoal => enemiesToDefeatGoal;

        public System.Action<int, int> OnProgressUpdated;
        public System.Action OnLevelCleared;

        private bool isLevelCleared = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            EnemyAI.OnEnemyDeath += HandleEnemyDeath;
        }

        private void OnDisable()
        {
            EnemyAI.OnEnemyDeath -= HandleEnemyDeath;
        }

        private void HandleEnemyDeath(EnemyAI enemy)
        {
            if (isLevelCleared) return;

            defeatedEnemiesCount++;
            OnProgressUpdated?.Invoke(defeatedEnemiesCount, enemiesToDefeatGoal);

            Debug.Log($"[LevelManager] Enemy defeated! Progress: {defeatedEnemiesCount}/{enemiesToDefeatGoal}");

            if (defeatedEnemiesCount >= enemiesToDefeatGoal)
            {
                ClearLevel();
            }
        }

        private void ClearLevel()
        {
            isLevelCleared = true;
            OnLevelCleared?.Invoke();
            Debug.Log("[LevelManager] LEVEL CLEARED! 20 enemies defeated.");
        }
    }
}