using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple procedural 2D "map" generator for a Vampire Survivors-like prototype.
/// - Builds outer boundary walls (collidable)
/// - Spawns a few internal obstacles (collidable)
/// - Generates simple sprites at runtime so everything is visible
///
/// Attach this to an empty GameObject named "Map".
/// </summary>
public class MapMono : MonoBehaviour
{
    [Header("Map Size (world units)")]
    [SerializeField] private Vector2 mapSize = new Vector2(30f, 18f);

    [Header("Wall")]
    [SerializeField] private float wallThickness = 1f;

    [Header("Internal Obstacles")]
    [SerializeField] private int obstacleCount = 8;
    [SerializeField] private Vector2 obstacleSizeMin = new Vector2(1.5f, 1.0f);
    [SerializeField] private Vector2 obstacleSizeMax = new Vector2(4.0f, 2.5f);
    [SerializeField] private int randomSeed = 12345;

    [Header("Visual")]
    [SerializeField] private int spritePixelsPerUnit = 32;

    private readonly List<GameObject> _spawned = new List<GameObject>();

    private void Start()
    {
        Generate();
    }

    [ContextMenu("Regenerate")]
    public void Generate()
    {
        ClearSpawned();

        // Outer bounds in world coordinates
        float halfW = mapSize.x * 0.5f;
        float halfH = mapSize.y * 0.5f;

        // Create 4 boundary walls: Top, Bottom, Left, Right
        // Each wall is a GameObject with BoxCollider2D + SpriteRenderer
        CreateWall("Wall_Top",
            pos: new Vector2(0f, halfH + wallThickness * 0.5f),
            size: new Vector2(mapSize.x + wallThickness * 2f, wallThickness));

        CreateWall("Wall_Bottom",
            pos: new Vector2(0f, -halfH - wallThickness * 0.5f),
            size: new Vector2(mapSize.x + wallThickness * 2f, wallThickness));

        CreateWall("Wall_Left",
            pos: new Vector2(-halfW - wallThickness * 0.5f, 0f),
            size: new Vector2(wallThickness, mapSize.y));

        CreateWall("Wall_Right",
            pos: new Vector2(halfW + wallThickness * 0.5f, 0f),
            size: new Vector2(wallThickness, mapSize.y));

        // Internal obstacles
        var rng = new System.Random(randomSeed);
        for (int i = 0; i < obstacleCount; i++)
        {
            Vector2 size = new Vector2(
                Lerp(obstacleSizeMin.x, obstacleSizeMax.x, (float)rng.NextDouble()),
                Lerp(obstacleSizeMin.y, obstacleSizeMax.y, (float)rng.NextDouble())
            );

            // Place inside the map area (avoid touching boundary)
            float padding = 1.5f;
            float x = Lerp(-halfW + padding, halfW - padding, (float)rng.NextDouble());
            float y = Lerp(-halfH + padding, halfH - padding, (float)rng.NextDouble());

            CreateObstacle($"Obstacle_{i:00}", new Vector2(x, y), size);
        }

        // Optional: visual floor (no collider)
        CreateFloor("Floor", Vector2.zero, mapSize);
    }

    private void CreateFloor(string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(pos.x, pos.y, 5f); // behind player (camera is -10 by default)

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSolidSprite(64, 64, new Color(0.12f, 0.12f, 0.14f, 1f));
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = size;
        sr.sortingOrder = -10;

        _spawned.Add(go);
    }

    private void CreateWall(string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(pos.x, pos.y, 0f);

        var col = go.AddComponent<BoxCollider2D>();
        col.size = size;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSolidSprite(32, 32, new Color(0.22f, 0.22f, 0.26f, 1f));
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = size;
        sr.sortingOrder = 0;

        _spawned.Add(go);
    }

    private void CreateObstacle(string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(pos.x, pos.y, 0f);

        var col = go.AddComponent<BoxCollider2D>();
        col.size = size;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSolidSprite(32, 32, new Color(0.35f, 0.32f, 0.28f, 1f));
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = size;
        sr.sortingOrder = 1;

        _spawned.Add(go);
    }

    private Sprite CreateSolidSprite(int w, int h, Color c)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, c);

        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), spritePixelsPerUnit);
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * Mathf.Clamp01(t);

    private void ClearSpawned()
    {
        for (int i = _spawned.Count - 1; i >= 0; i--)
        {
            if (_spawned[i] != null)
            {
                Destroy(_spawned[i]);
            }
        }
        _spawned.Clear();
    }
}