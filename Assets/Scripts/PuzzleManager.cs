using UnityEngine;
using System.Collections.Generic;

namespace WarriorPlatformer
{
    public class PuzzleManager : MonoBehaviour
    {
        [Header("Puzzle Settings")]
        [SerializeField] private List<PuzzleMechanism> mechanisms = new List<PuzzleMechanism>();
        [SerializeField] private GameObject bossGate;
        [SerializeField] private string completionMessage = "Ancient Seal Broken! The Guardian Awaits.";

        public System.Action<int, int> OnPuzzleProgressUpdated;
        public System.Action OnPuzzleCompleted;

        private bool isCompleted = false;

        private void Start()
        {
            foreach (var mechanism in mechanisms)
            {
                if (mechanism != null)
                {
                    mechanism.OnStateChanged += HandleMechanismStateChanged;
                }
            }
            
            UpdateProgress();
        }

        private void OnDestroy()
        {
            foreach (var mechanism in mechanisms)
            {
                if (mechanism != null)
                {
                    mechanism.OnStateChanged -= HandleMechanismStateChanged;
                }
            }
        }

        private void HandleMechanismStateChanged(bool state)
        {
            if (isCompleted) return;
            
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            int activeCount = 0;
            foreach (var m in mechanisms)
            {
                if (m != null && m.IsActivated) activeCount++;
            }

            OnPuzzleProgressUpdated?.Invoke(activeCount, mechanisms.Count);

            if (activeCount >= mechanisms.Count && mechanisms.Count > 0)
            {
                CompletePuzzle();
            }
        }

        private void CompletePuzzle()
        {
            isCompleted = true;
            
            if (bossGate != null)
            {
                bossGate.SetActive(false); // Or trigger an animation
            }

            OnPuzzleCompleted?.Invoke();
            Debug.Log($"[PuzzleManager] {completionMessage}");
        }
    }
}
