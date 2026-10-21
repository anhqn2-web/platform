using UnityEngine;

namespace WarriorPlatformer
{
    public class ToggleSwitch : PuzzleMechanism
    {
        [Header("Toggle Switch Settings")]
        [SerializeField] private float interactRange = 1.5f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        private bool playerInRange = false;

        private void Update()
        {
            if (playerInRange && Input.GetKeyDown(interactKey))
            {
                SetActivated(!isActivated);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
            }
        }
    }
}
