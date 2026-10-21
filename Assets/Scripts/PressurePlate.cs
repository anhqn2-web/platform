using UnityEngine;

namespace WarriorPlatformer
{
    public class PressurePlate : PuzzleMechanism
    {
        [Header("Pressure Plate Settings")]
        [SerializeField] private LayerMask activationLayers;
        [SerializeField] private float pressDuration = 0.1f;
        
        private int objectsOnPlate = 0;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & activationLayers) != 0)
            {
                objectsOnPlate++;
                SetActivated(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & activationLayers) != 0)
            {
                objectsOnPlate--;
                if (objectsOnPlate <= 0)
                {
                    objectsOnPlate = 0;
                    SetActivated(false);
                }
            }
        }
    }
}
