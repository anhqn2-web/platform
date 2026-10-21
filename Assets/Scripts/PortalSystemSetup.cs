using UnityEngine;

namespace WarriorPlatformer
{
    public class PortalSystemSetup : MonoBehaviour
    {
        [Header("Setup Configuration")]
        [SerializeField] private string targetGameObjectName = "Tower_End";
        [SerializeField] private Sprite portalSprite;
        [SerializeField] private Sprite guidanceSprite;
        [SerializeField] private Material portalMaterial;

        private void Awake()
        {
            // If sprites not assigned, try to load from known paths
            if (portalSprite == null) portalSprite = Resources.Load<Sprite>("Asset FTTGR/Sprites/VFXs/Light");
            if (guidanceSprite == null) guidanceSprite = Resources.Load<Sprite>("Asset FTTGR/Sprites/VFXs/Circle");
            
            SetupPortal();
        }

        public void SetupPortal()
        {
            GameObject towerEnd = GameObject.Find(targetGameObjectName);
            if (towerEnd == null)
            {
                Debug.LogWarning($"[PortalSetup] Could not find GameObject named '{targetGameObjectName}'. Creating a fallback exit at (100, 7.5, 0).");
                towerEnd = new GameObject(targetGameObjectName);
                towerEnd.transform.position = new Vector3(100f, 7.5f, 0f);
            }

            // 1. Setup Portal Visuals and Trigger
            SetupPortalObject(towerEnd);

            // 2. Setup Guidance System
            SetupGuidanceSystem(towerEnd.transform);
        }

        private void SetupPortalObject(GameObject portalObj)
        {
            // Add SpriteRenderer if missing
            SpriteRenderer sr = portalObj.GetComponent<SpriteRenderer>();
            if (sr == null) sr = portalObj.AddComponent<SpriteRenderer>();
            
            // If sprite is null, we recommend the user to drag 'Light.png' from 'Asset FTTGR/Sprites/VFXs'
            if (sr.sprite == null && portalSprite != null)
            {
                sr.sprite = portalSprite;
            }
            
            sr.sortingOrder = 10; // Ensure it's in front
            sr.color = new Color(0.2f, 0.2f, 0.2f, 0.5f); // Initially dim
            sr.transform.localScale = new Vector3(2f, 2f, 1f);

            // Add BoxCollider2D if missing
            BoxCollider2D col = portalObj.GetComponent<BoxCollider2D>();
            if (col == null) col = portalObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.5f, 2.5f);

            // Add PortalController
            PortalController pc = portalObj.GetComponent<PortalController>();
            if (pc == null) pc = portalObj.AddComponent<PortalController>();
            
            // Setup Particle System (Optional, simple setup)
            ParticleSystem ps = portalObj.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                ps = portalObj.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startColor = new Color(0f, 0.8f, 1f, 0.5f);
                main.startSize = 0.2f;
                main.startLifetime = 1f;
                main.loop = true;
                main.playOnAwake = false;
                
                var emission = ps.emission;
                emission.rateOverTime = 20;
                
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 0.5f;
            }

            // Setup Light (Optional)
            Light light = portalObj.GetComponent<Light>();
            if (light == null)
            {
                light = portalObj.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 5f;
                light.intensity = 2f;
                light.color = new Color(0f, 0.8f, 1f, 1f);
                light.enabled = false;
            }

            pc.SetupComponents(sr, ps, light);
        }

        private void SetupGuidanceSystem(Transform portalTransform)
        {
            GameObject guidanceObj = new GameObject("PortalGuidanceIndicator");
            PortalGuidance pg = guidanceObj.AddComponent<PortalGuidance>();
            pg.SetTarget(portalTransform);

            GameObject visualObj = new GameObject("Visual");
            visualObj.transform.SetParent(guidanceObj.transform);
            visualObj.transform.localPosition = Vector3.zero;
            
            SpriteRenderer sr = visualObj.AddComponent<SpriteRenderer>();
            // Use Circle sprite if possible, stretched to look like an arrow
            if (guidanceSprite != null) sr.sprite = guidanceSprite;
            
            sr.color = new Color(0f, 1f, 0.8f, 1f);
            sr.sortingOrder = 100;
            visualObj.transform.localScale = new Vector3(0.5f, 0.2f, 1f); // Arrow-like shape
        }
    }
}
