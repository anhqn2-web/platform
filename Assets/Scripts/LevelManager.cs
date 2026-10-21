using UnityEngine;

namespace WarriorPlatformer
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Level Settings")]
        [SerializeField] private string questTitle = "Clearing the Path";
        [SerializeField] private string questDescription = "Defeat the enemies blocking your progress.";
        [SerializeField] private bool useDefeatCountGoal = true;
        [SerializeField] private int enemiesToDefeatGoal = 20;
        [SerializeField] private int defeatedEnemiesCount = 0;

        [Header("Boss Settings")]
        [SerializeField] private bool requiresBossDefeat = false;
        [SerializeField] private EnemyAI bossEnemy;

        public string QuestTitle => questTitle;
        public string QuestDescription => questDescription;
        public int DefeatedEnemiesCount => defeatedEnemiesCount;
        public int EnemiesToDefeatGoal => enemiesToDefeatGoal;

        public System.Action<int, int> OnProgressUpdated;
        public System.Action OnLevelCleared;

        private bool isLevelCleared = false;
        private bool isBossDefeated = false;

        public bool IsLevelCleared => isLevelCleared;

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

            if (enemy == bossEnemy)
            {
                isBossDefeated = true;
                Debug.Log("[LevelManager] Boss defeated!");
            }

            defeatedEnemiesCount++;
            OnProgressUpdated?.Invoke(defeatedEnemiesCount, enemiesToDefeatGoal);

            CheckCompletion();
        }

        private void CheckCompletion()
        {
            bool enemiesCleared = !useDefeatCountGoal || (defeatedEnemiesCount >= enemiesToDefeatGoal);
            bool bossCleared = !requiresBossDefeat || isBossDefeated;

            if (enemiesCleared && bossCleared)
            {
                ClearLevel();
            }
        }

        private void ClearLevel()
        {
            isLevelCleared = true;
            OnLevelCleared?.Invoke();
            Debug.Log("[LevelManager] LEVEL CLEARED!");
        }

        public void SetBoss(EnemyAI boss)
        {
            bossEnemy = boss;
            requiresBossDefeat = true;
        }
    }
}