using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Linq;

namespace WarriorPlatformer.Editor
{
    public class LevelGenerator : EditorWindow
    {
        [MenuItem("Tools/Generate Winter Level")]
        public static void GenerateLevel()
        {
            // 0. Setup Scene
            GameObject levelRoot = new GameObject("GeneratedLevel");

            // 1. Create Grid
            GameObject gridGO = new GameObject("Grid");
            gridGO.transform.SetParent(levelRoot.transform);
            Grid grid = gridGO.AddComponent<Grid>();

            // 2. Create Tilemaps
            Tilemap groundTilemap = CreateTilemap(gridGO, "Ground", 10, "ground", "Default", true);
            Tilemap platformTilemap = CreateTilemap(gridGO, "Platforms", 11, "ground", "Default", true);
            Tilemap backgroundTilemap = CreateTilemap(gridGO, "Background_Tiles", -5, "Default", "Background_Mid", false);

            // 3. Load Assets
            Tile groundMain = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Asset FTTGR/Palettes/Blocks/GroundWinter1.asset");
            Tile groundTop = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Asset FTTGR/Palettes/Blocks/GroundWinter10.asset");
            Tile brickMain = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Asset FTTGR/Palettes/Blocks/Bricks1.asset");

            // 4. Generate Terrain
            FillRect(groundTilemap, groundMain, -15, -10, 30, 10);
            FillRect(groundTilemap, groundTop, -15, 0, 30, 1);

            FillPlatform(platformTilemap, brickMain, 18, 3, 5);
            FillPlatform(platformTilemap, brickMain, 25, 6, 6);
            FillPlatform(platformTilemap, brickMain, 15, 8, 4);

            FillRect(groundTilemap, groundMain, 35, -10, 15, 10);
            FillRect(groundTilemap, groundTop, 35, 0, 15, 1);

            FillPlatform(platformTilemap, brickMain, 55, 3, 4);
            FillPlatform(platformTilemap, brickMain, 62, 7, 5);

            FillRect(groundTilemap, groundMain, 75, -10, 40, 10);
            FillRect(groundTilemap, groundTop, 75, 0, 40, 1);

            // 5. Decorations
            PlaceDecorations(levelRoot.transform);

            // 6. Gameplay Systems
            SetupGameplay(levelRoot.transform);

            // 7. Background
            SetupBackground(levelRoot.transform);

            Debug.Log("Winter Level Generation Complete!");
        }

        private static Tilemap CreateTilemap(GameObject parent, string name, int sortingOrder, string layerName, string sortingLayerName, bool addCollider)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            int layer = LayerMask.NameToLayer(layerName);
            if (layer != -1) go.layer = layer;
            
            Tilemap tilemap = go.AddComponent<Tilemap>();
            TilemapRenderer renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = sortingOrder;
            renderer.sortingLayerName = sortingLayerName;
            
            if (addCollider)
            {
                TilemapCollider2D col = go.AddComponent<TilemapCollider2D>();
                Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;
                go.AddComponent<CompositeCollider2D>();
                col.usedByComposite = true;
            }
            return tilemap;
        }

        private static void FillRect(Tilemap tilemap, TileBase tile, int x, int y, int width, int height)
        {
            if (tile == null) return;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    tilemap.SetTile(new Vector3Int(x + i, y + j, 0), tile);
                }
            }
        }

        private static void FillPlatform(Tilemap tilemap, TileBase tile, int x, int y, int width)
        {
            FillRect(tilemap, tile, x, y, width, 1);
        }

        private static void PlaceDecorations(Transform parent)
        {
            // Trees (Background/Midground)
            Object[] treeAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Asset FTTGR/Sprites/Environment/Grounds/Trees.png");
            Sprite tree1 = treeAssets.OfType<Sprite>().FirstOrDefault(s => s.name == "Tree1");
            Sprite tree3 = treeAssets.OfType<Sprite>().FirstOrDefault(s => s.name == "Tree3");

            if (tree1 != null)
            {
                CreateSprite(parent, "Tree_Start", tree1, new Vector3(-5, 3.5f, 0), 5);
                CreateSprite(parent, "Tree_Middle", tree3, new Vector3(42, 3.5f, 0), 5);
                CreateSprite(parent, "Tree_End", tree1, new Vector3(85, 3.5f, 0), 5);
            }

            // Lamp Posts
            Object[] elseAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Asset FTTGR/Sprites/Environment/Decorations/Else.png");
            Sprite lampSprite = elseAssets.OfType<Sprite>().FirstOrDefault(s => s.name == "LampPostLit");
            if (lampSprite == null) lampSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Asset FTTGR/Sprites/Environment/Grounds/LampPost.png");
            
            if (lampSprite != null)
            {
                CreateSprite(parent, "Lamp1", lampSprite, new Vector3(5, 3.5f, 0), 6);
                CreateSprite(parent, "Lamp2", lampSprite, new Vector3(45, 3.5f, 0), 6);
                CreateSprite(parent, "Lamp3", lampSprite, new Vector3(80, 3.5f, 0), 6);
            }

            // Industrial Details
            Sprite roadSign = elseAssets.OfType<Sprite>().FirstOrDefault(s => s.name == "RoadSign2");
            Sprite box = elseAssets.OfType<Sprite>().FirstOrDefault(s => s.name == "Box1");
            Sprite barrel = elseAssets.OfType<Sprite>().FirstOrDefault(s => s.name == "Barrel1");
            Sprite tower = elseAssets.OfType<Sprite>().FirstOrDefault(s => s.name == "Tower");
            Sprite fence = elseAssets.OfType<Sprite>().FirstOrDefault(s => s.name == "Fence1");

            if (roadSign != null) CreateSprite(parent, "Sign_Start", roadSign, new Vector3(10, 1.5f, 0), 6);
            if (box != null) CreateSprite(parent, "Box_Start", box, new Vector3(12, 0.8f, 0), 6);
            if (barrel != null) CreateSprite(parent, "Barrel_Start", barrel, new Vector3(13, 0.8f, 0), 6);
            if (fence != null) CreateSprite(parent, "Fence_Start", fence, new Vector3(15, 1.2f, 0), 6);
            if (tower != null) 
            {
                GameObject t = CreateSprite(parent, "Tower_End", tower, new Vector3(100, 7.5f, 0), 4);
                t.transform.localScale = new Vector3(2, 2, 1);
            }

            // Foreground Silhouettes
            Sprite tree6 = treeAssets.OfType<Sprite>().FirstOrDefault(s => s.name == "Tree6");
            if (tree6 != null)
            {
                GameObject fg = CreateSprite(parent, "FG_Tree", tree6, new Vector3(20, -1, -5), 25);
                SpriteRenderer sr = fg.GetComponent<SpriteRenderer>();
                sr.color = new Color(0.05f, 0.05f, 0.1f, 1f); // Dark silhouette
                fg.transform.localScale = new Vector3(3, 3, 1);
            }
        }

        private static GameObject CreateSprite(Transform parent, string name, Sprite sprite, Vector3 position, int sortingOrder)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            return go;
        }

        private static void SetupGameplay(Transform parent)
        {
            // Player
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/cbiSan/Player.prefab");
            if (playerPrefab != null)
            {
                GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                player.transform.position = new Vector3(0, 2, 0);
                player.transform.SetParent(parent);
            }

            // Camera
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0, 2, -10);
                // Ensure CameraFollow is assigned
                var follow = cam.GetComponent<WarriorPlatformer.CameraFollow>();
                if (follow == null) follow = cam.gameObject.AddComponent<WarriorPlatformer.CameraFollow>();
                
                // We'll need to find the player at runtime or assign it here if we can
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) {
                    // Using reflection to set private field if necessary, or just rely on the script finding it
                }
            }

            // Manager & Spawner
            GameObject levelManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/cbiSan/LevelManager.prefab");
            if (levelManagerPrefab != null) {
                GameObject lm = (GameObject)PrefabUtility.InstantiatePrefab(levelManagerPrefab);
                lm.transform.SetParent(parent);
            }

            GameObject enemySpawnerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/cbiSan/EnemySpawner.prefab");
            if (enemySpawnerPrefab != null) {
                GameObject es = (GameObject)PrefabUtility.InstantiatePrefab(enemySpawnerPrefab);
                es.transform.SetParent(parent);
            }
        }

        private static void SetupBackground(Transform parent)
        {
            GameObject bgPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/cbiSan/MainBackground2.prefab");
            if (bgPrefab != null)
            {
                // Create multiple backgrounds for a long level
                for (int i = 0; i < 5; i++)
                {
                    GameObject bg = (GameObject)PrefabUtility.InstantiatePrefab(bgPrefab);
                    bg.transform.SetParent(parent);
                    bg.transform.position = new Vector3(i * 40 - 20, 5, 20);
                }
            }
        }
    }
}
