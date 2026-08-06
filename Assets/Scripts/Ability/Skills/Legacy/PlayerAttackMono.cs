using UnityEngine;

/// <summary>
/// Handles player projectile attack (missile-like).
/// Projectile has:
/// - Speed
/// - Max travel distance
/// - Lifetime
///
/// Also supports dealing damage to NPCs only once per NPC per projectile.
/// </summary>
public class PlayerAttackMono : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileDistance = 8f;
    [SerializeField] private float projectileLifetime = 3f;

    [Header("Damage")]
    [SerializeField] private int damagePerHit = 1;

    [SerializeField] private KeyCode fireKey = KeyCode.Space;

    private void Update()
    {
        if (Input.GetKeyDown(fireKey))
        {
            Fire();
        }
    }

    private void Fire()
    {
        GameObject proj = new GameObject("Projectile");
        proj.transform.position = transform.position;

        var sr = proj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSprite(16, 16);
        sr.sortingOrder = 5;

        // For trigger collisions, at least one side needs Rigidbody2D.
        // Kinematic is fine for a projectile we move ourselves.
        var rb = proj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = proj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.25f;

        var projectile = proj.AddComponent<ProjectileMono>();
        projectile.Initialize(
            dir: transform.up, // 2D top-down: up is forward
            speed: projectileSpeed,
            maxDist: projectileDistance,
            lifetime: projectileLifetime,
            damage: damagePerHit
        );
    }

    private Sprite CreateSprite(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color c = new Color(1f, 0.3f, 0.1f, 1f);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, c);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }
}
