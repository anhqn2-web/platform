using UnityEngine;
using System.Collections;

namespace WarriorPlatformer
{
    public abstract class PuzzleMechanism : MonoBehaviour
    {
        [Header("Puzzle Mechanism Settings")]
        [SerializeField] protected bool isActivated = false;
        
        public bool IsActivated => isActivated;
        
        public System.Action<bool> OnStateChanged;

        protected virtual void SetActivated(bool state)
        {
            if (isActivated == state) return;
            
            isActivated = state;
            OnStateChanged?.Invoke(isActivated);
            
            Debug.Log($"[PuzzleMechanism] {gameObject.name} state changed to: {isActivated}");
        }
    }
}
