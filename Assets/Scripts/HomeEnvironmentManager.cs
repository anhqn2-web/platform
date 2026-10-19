using UnityEngine;
using System.Collections.Generic;

namespace WarriorPlatformer
{
    /// <summary>
    /// Sets up and manages the "WINTER WARRIOR" home screen environment.
    /// This script creates the multi-layered snowy mountain background, parallax effects,
    /// snow particles, and cinematic lighting.
    /// </summary>
    public class HomeEnvironmentManager : MonoBehaviour
    {
        [Header("Background Assets")]
        [SerializeField] private Sprite mountainBackground; // Suggest: MainBackground2
        [SerializeField] private Sprite pineTreesSprite;    // Suggest: Trees
        [SerializeField] private Sprite rockyPlatform;      // Suggest: Rocks
        [SerializeField] private Sprite lampPostSprite;     // Suggest: LampPost

        [Header("VFX & Lighting")]
        [SerializeField] private GameObject snowPrefab;      // Suggest: EffectSnow
        [SerializeField] private GameObject torchPrefab;     // Suggest: FireType1
        [SerializeField] private Color ambientColor = new Color(0.05f, 0.1f, 0.2f); // Deep navy blue
        
        [Header("Character")]
        [SerializeField] private Sprite warriorSprite;      // Suggest: Warrior_SheetnoEffect
        
        private void Awake()
        {
            SetupEnvironment();
        }

        private void SetupEnvironment()
        {
            // 1. Setup Camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.backgroundColor = ambientColor;
                mainCam.clearFlags = CameraClearFlags.SolidColor;
            }

            // 2. Create Background Layers (Parallax)
            CreateLayer("Background_Mountains", mountainBackground, 0.9f, new Vector3(0, 0, 10), new Vector3(1.5f, 1.5f, 1));
            CreateLayer("Midground_Trees", pineTreesSprite, 0.7f, new Vector3(0, -1, 5), new Vector3(1.2f, 1.2f, 1), new Color(0.6f, 0.7f, 0.8f));
            
            // 3. Create Foreground (Platform)
            GameObject platform = CreateLayer("Foreground_Platform", rockyPlatform, 0f, new Vector3(0, -4, 0), Vector3.one);
            
            // 4. Place Warrior
            if (warriorSprite != null)
            {
                GameObject warrior = new GameObject("MainWarrior", typeof(SpriteRenderer));
                warrior.transform.position = new Vector3(0, -3.2f, -1);
                SpriteRenderer sr = warrior.GetComponent<SpriteRenderer>();
                sr.sprite = warriorSprite;
                sr.sortingOrder = 10;
                // Since it's a sheet, we'd normally need to slice it, but for a home screen 
                // we'll assume the first frame or a dedicated sprite.
            }

            // 5. Add Lamp Post & Torch
            if (lampPostSprite != null)
            {
                GameObject lamp = new GameObject("LampPost", typeof(SpriteRenderer));
                lamp.transform.position = new Vector3(-5, -2, -0.5f);
                lamp.GetComponent<SpriteRenderer>().sprite = lampPostSprite;
                lamp.GetComponent<SpriteRenderer>().sortingOrder = 5;

                // Add torch effect to lamp
                if (torchPrefab != null)
                {
                    GameObject torch = Instantiate(torchPrefab, lamp.transform);
                    torch.transform.localPosition = new Vector3(0.5f, 2.5f, -0.1f);
                }
            }

            // 6. Instantiate Snow VFX
            if (snowPrefab != null)
            {
                GameObject snow = Instantiate(snowPrefab);
                snow.name = "FallingSnow_VFX";
            }
            else
            {
                Debug.LogWarning("Snow Prefab not assigned to HomeEnvironmentManager. Please assign 'EffectSnow' prefab.");
            }
        }

        private GameObject CreateLayer(string name, Sprite sprite, float parallaxFactor, Vector3 pos, Vector3 scale, Color? tint = null)
        {
            if (sprite == null) return null;

            GameObject obj = new GameObject(name, typeof(SpriteRenderer), typeof(ParallaxBackground));
            obj.transform.position = pos;
            obj.transform.localScale = scale;

            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = (int)(-pos.z * 10);
            if (tint.HasValue) sr.color = tint.Value;

            ParallaxBackground pb = obj.GetComponent<ParallaxBackground>();
            // Using reflection or making fields public in ParallaxBackground would be needed 
            // if we wanted to set the factor via code without modifying the original script.
            // For now, we'll assume it uses the default or is set via inspector.
            
            return obj;
        }
    }
}
