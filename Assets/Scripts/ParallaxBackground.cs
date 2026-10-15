using UnityEngine;

namespace WarriorPlatformer
{
    /// <summary>
    /// Simple parallax script to make background layers move at different speeds relative to the camera.
    /// This adds a sense of depth to your 2D level.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("The camera to track. If empty, it finds the Main Camera.")]
        [SerializeField] private Transform cameraTransform;
        
        [Tooltip("How much this layer moves relative to the camera. 0 = stays with camera, 1 = static in world.")]
        [Range(0f, 1f)]
        [SerializeField] private float parallaxFactor = 0.5f;

        private Vector3 lastCameraPosition;
        private float textureUnitSizeX;

        private void Start()
        {
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main.transform;
            }

            lastCameraPosition = cameraTransform.position;

            // Optional: Support for infinite scrolling backgrounds
            Sprite sprite = GetComponent<SpriteRenderer>().sprite;
            if (sprite != null)
            {
                Texture2D texture = sprite.texture;
                textureUnitSizeX = texture.width / sprite.pixelsPerUnit;
            }
        }

        private void LateUpdate()
        {
            // Calculate movement delta
            Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
            
            // Apply parallax effect (multiply camera movement by factor)
            transform.position += new Vector3(deltaMovement.x * parallaxFactor, deltaMovement.y * parallaxFactor, 0);
            
            lastCameraPosition = cameraTransform.position;

            // Infinite Scrolling Logic (Optional)
            if (Mathf.Abs(cameraTransform.position.x - transform.position.x) >= textureUnitSizeX)
            {
                float offsetPositionX = (cameraTransform.position.x - transform.position.x) % textureUnitSizeX;
                transform.position = new Vector3(cameraTransform.position.x + offsetPositionX, transform.position.y);
            }
        }
    }
}
