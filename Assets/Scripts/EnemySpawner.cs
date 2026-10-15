using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WarriorPlatformer
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawner Settings")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private float spawnInterval = 3f;
        [SerializeField] private int maxActiveEnemies = 5;

        private List<EnemyAI> activeEnemies = new List<EnemyAI>();
        private bool isSpawning = true;

        private void Start()
        {
            StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            while (isSpawning)
            {
                // Wait for the next spawn interval
                yield return new WaitForSeconds(spawnInterval);

                // Stop spawning if level goal is reached (optional check here, but LevelManager handles goal)
                if (LevelManager.Instance != null && LevelManager.Instance.DefeatedEnemiesCount >= LevelManager.Instance.EnemiesToDefeatGoal)
                {
                    isSpawning = false;
                    break;
                }

                // Cleanup missing references (destroyed enemies)
                activeEnemies.RemoveAll(e => e == null);

                // Spawn if we haven't hit the limit
                if (activeEnemies.Count < maxActiveEnemies)
                {
                    SpawnEnemy();
                }
            }
        }

        private void SpawnEnemy()
        {
            if (enemyPrefab == null)
            {
                Debug.LogWarning("[EnemySpawner] No enemy prefab assigned!");
                return;
            }

            GameObject newEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            EnemyAI ai = newEnemy.GetComponent<EnemyAI>();
            if (ai != null)
            {
                activeEnemies.Add(ai);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
            Gizmos.DrawIcon(transform.position, "d_Prefab Icon", true);
        }
    }
}
