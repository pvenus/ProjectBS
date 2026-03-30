using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Party bootstrapper.
/// - Spawns party members
/// - Registers them into PartyControlManager
/// - No longer acts as a directly playable character
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class PlayerMono : MonoBehaviour
{
    [Header("Progression")]
    [SerializeField] private int level = 1;
    [SerializeField] private int exp = 0;
    [SerializeField] private int expToNext = 100;
    [Tooltip("Extra EXP required per level up.")]
    [SerializeField] private int expGrowthPerLevel = 3;

    [Header("Visual")]
    [SerializeField] private int spriteSize = 64;

    private SpriteRenderer _sr;
    private Rigidbody2D _rb;

    [Header("Party")]
    [SerializeField] private int partyMemberTankCount = 1;
    [SerializeField] private int partyMemberHealCount = 1;
    [SerializeField] private int partyMemberDPSCount = 1;
    [SerializeField] private TowerPropMono partySpawnTower;
    [SerializeField] private Vector3 partySpawnOffset = Vector3.zero;
    [SerializeField] private float partySpawnSpacing = 1.5f;
    [SerializeField] private bool hideBootstrapVisual = true;
    [SerializeField] private bool disableBootstrapCollision = true;
    [SerializeField] private PartyControlManager partyControlManager;

    [Header("Camera")]
    [SerializeField] private bool followCurrentPartyMember = true;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 0f, -10f);
    [SerializeField] private bool snapCameraOnStart = true;
    [SerializeField] private bool debugCameraFollow = false;
    [SerializeField] private float debugCameraLogInterval = 0.5f;
    private GameObject _partyMemberTankPrefab;
    private GameObject _partyMemberHealPrefab;
    private GameObject _partyMemberDPSPrefab;
    private float _debugCameraLogTimer = 0f;
    private Vector3 _lastLoggedCameraTarget = Vector3.zero;

    private void Start()
    {
        // Force-add required components (covers cases where RequireComponent didn't retroactively apply)
        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();

        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null) _rb = gameObject.AddComponent<Rigidbody2D>();

        var cc = GetComponent<CircleCollider2D>();
        if (cc == null) cc = gameObject.AddComponent<CircleCollider2D>();

        // This runs when the component is first added or reset in the editor.
        // Make sure required components are configured sensibly.
        if (_rb != null)
        {
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        if (cc != null)
        {
            cc.isTrigger = false;
            cc.radius = 0.45f;
        }

        if (hideBootstrapVisual && _sr != null)
            _sr.enabled = false;

        if (disableBootstrapCollision)
        {
            if (cc != null)
                cc.enabled = false;

            if (_rb != null)
                _rb.simulated = false;
        }

        if (partyControlManager == null)
            partyControlManager = GetComponent<PartyControlManager>();

        // Load party member prefab from Resources and spawn them
        _partyMemberTankPrefab = Resources.Load<GameObject>("PartyMemberTank");
        _partyMemberHealPrefab = Resources.Load<GameObject>("PartyMemberHeal");
        _partyMemberDPSPrefab = Resources.Load<GameObject>("PartyMemberDPS");

        if (_partyMemberTankPrefab == null && _partyMemberHealPrefab == null && _partyMemberDPSPrefab == null)
            Debug.LogWarning("Party member prefabs were not found in the Resources folder.");

        List<PartyMovementMono> spawnedMembers = new List<PartyMovementMono>();
        int spawnIndex = 0;

        SpawnPartyMembers(_partyMemberTankPrefab, partyMemberTankCount, spawnedMembers, ref spawnIndex);
        SpawnPartyMembers(_partyMemberHealPrefab, partyMemberHealCount, spawnedMembers, ref spawnIndex);
        SpawnPartyMembers(_partyMemberDPSPrefab, partyMemberDPSCount, spawnedMembers, ref spawnIndex);

        if (partyControlManager != null)
            partyControlManager.SetMembers(spawnedMembers);
        else if (spawnedMembers.Count > 0)
            Debug.LogWarning("PartyControlManager was not assigned, so spawned members were not registered for player control.");

        if (snapCameraOnStart)
            SnapCameraToCurrentMember();
    }

    private void LateUpdate()
    {
        if (!followCurrentPartyMember)
            return;

        FollowCurrentPartyMemberCamera();

        if (debugCameraFollow)
            DebugCurrentPartyMemberCamera();
    }

    private void OnValidate()
    {
        // Ensure required components exist in Edit Mode as well.
        if (!Application.isPlaying)
        {
            if (GetComponent<SpriteRenderer>() == null) gameObject.AddComponent<SpriteRenderer>();
            if (GetComponent<Rigidbody2D>() == null) gameObject.AddComponent<Rigidbody2D>();
            if (GetComponent<CircleCollider2D>() == null) gameObject.AddComponent<CircleCollider2D>();
        }

        // Make the generated sprite appear even in Edit Mode (so you see something without pressing Play).
        // Note: this creates a runtime texture in memory (fine for prototyping).
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        if (_sr != null && _sr.sprite == null)
        {
            _sr.sprite = CreateSprite(spriteSize, spriteSize);
        }

        if (partyControlManager == null)
            partyControlManager = GetComponent<PartyControlManager>();
    }

    private void Awake()
    {
        // Force-add required components at runtime too.
        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();

        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null) _rb = gameObject.AddComponent<Rigidbody2D>();

        var cc = GetComponent<CircleCollider2D>();
        if (cc == null) cc = gameObject.AddComponent<CircleCollider2D>();
        cc.isTrigger = false;

        // If no sprite is assigned in the editor, generate one so the player is visible.
        if (_sr.sprite == null)
        {
            _sr.sprite = CreateSprite(spriteSize, spriteSize);
        }

        // Rigidbody2D setup for top-down movement.
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Optional: prevent the rigidbody from being pushed by other dynamics (for now)
        // You can remove this if you later add knockback/physics.
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void SpawnPartyMembers(GameObject prefab, int count, List<PartyMovementMono> spawnedMembers, ref int spawnIndex)
    {
        if (prefab == null || count <= 0)
            return;

        Vector3 baseCenter = partySpawnTower != null ? partySpawnTower.transform.position : transform.position;
        baseCenter += partySpawnOffset;

        Vector3[] cardinalOffsets = new Vector3[]
        {
            new Vector3(0f, partySpawnSpacing, 0f),
            new Vector3(partySpawnSpacing, 0f, 0f),
            new Vector3(0f, -partySpawnSpacing, 0f),
            new Vector3(-partySpawnSpacing, 0f, 0f)
        };

        for (int i = 0; i < count; i++)
        {
            int ring = spawnIndex / 4;
            int slot = spawnIndex % 4;
            float ringMultiplier = ring + 1;
            Vector3 spawnPos = baseCenter + cardinalOffsets[slot] * ringMultiplier;

            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);
            PartyMovementMono move = obj.GetComponent<PartyMovementMono>();
            if (move != null)
                spawnedMembers.Add(move);

            spawnIndex++;
        }
    }

    private void FollowCurrentPartyMemberCamera()
    {
        Camera cam = Camera.main;
        if (cam == null || partyControlManager == null)
            return;

        PartyMovementMono currentMember = partyControlManager.GetCurrentMember();
        if (currentMember == null)
            return;

        Vector3 targetPos = currentMember.transform.position + cameraOffset;
        cam.transform.position = targetPos;
    }

    private void DebugCurrentPartyMemberCamera()
    {
        _debugCameraLogTimer -= Time.deltaTime;
        if (_debugCameraLogTimer > 0f)
            return;

        _debugCameraLogTimer = Mathf.Max(0.05f, debugCameraLogInterval);

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.Log("[CameraDebug] MainCamera not found.");
            return;
        }

        if (partyControlManager == null)
        {
            Debug.Log("[CameraDebug] PartyControlManager is null.");
            return;
        }

        PartyMovementMono currentMember = partyControlManager.GetCurrentMember();
        if (currentMember == null)
        {
            Debug.Log("[CameraDebug] Current member is null.");
            return;
        }

        Rigidbody2D memberRb = currentMember.GetComponent<Rigidbody2D>();
        Vector3 memberTransformPos = currentMember.transform.position;
        Vector2 memberRbPos = memberRb != null ? memberRb.position : Vector2.zero;
        Vector3 cameraTargetPos = memberTransformPos + cameraOffset;
        Vector3 cameraPos = cam.transform.position;
        float cameraDistance = Vector3.Distance(cameraPos, cameraTargetPos);
        float targetDelta = Vector3.Distance(_lastLoggedCameraTarget, cameraTargetPos);

        Debug.Log(
            $"[CameraDebug] member={currentMember.name}, " +
            $"memberTransform={memberTransformPos}, " +
            $"memberRb={(memberRb != null ? memberRbPos.ToString() : "null")}, " +
            $"camera={cameraPos}, " +
            $"target={cameraTargetPos}, " +
            $"camToTarget={cameraDistance:F3}, " +
            $"targetDeltaSinceLastLog={targetDelta:F3}, " +
            $"followMode=snap"
        );

        _lastLoggedCameraTarget = cameraTargetPos;
    }

    private void OnDrawGizmosSelected()
    {
        if (!debugCameraFollow)
            return;

        if (partyControlManager == null)
            return;

        PartyMovementMono currentMember = partyControlManager.GetCurrentMember();
        if (currentMember == null)
            return;

        Gizmos.color = Color.magenta;
        Vector3 targetPos = currentMember.transform.position + cameraOffset;
        Gizmos.DrawWireSphere(targetPos, 0.2f);
        Gizmos.DrawLine(currentMember.transform.position, targetPos);

        Camera cam = Camera.main;
        if (cam != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(cam.transform.position, 0.2f);
            Gizmos.DrawLine(cam.transform.position, targetPos);
        }
    }

    private void SnapCameraToCurrentMember()
    {
        Camera cam = Camera.main;
        if (cam == null || partyControlManager == null)
            return;

        PartyMovementMono currentMember = partyControlManager.GetCurrentMember();
        if (currentMember == null)
            return;

        cam.transform.position = currentMember.transform.position + cameraOffset;
    }

    private static Sprite CreateSprite(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        // Draw a simple yellow square with a dark border.
        Color fill = new Color(1f, 0.92f, 0.2f, 1f);
        Color border = new Color(0.15f, 0.12f, 0.05f, 1f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool isBorder = (x == 0 || y == 0 || x == w - 1 || y == h - 1 || x == 1 || y == 1 || x == w - 2 || y == h - 2);
                tex.SetPixel(x, y, isBorder ? border : fill);
            }
        }

        tex.Apply();

        // 16 pixels per unit gives a reasonably sized character in 2D.
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }
    public void AddExp(int amount)
    {
        if (amount <= 0) return;

        exp += amount;
        while (exp >= expToNext)
        {
            exp -= expToNext;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        expToNext += expGrowthPerLevel;

        // TODO: later hook stats upgrades here (damage, speed, etc.)
        Debug.Log($"LEVEL UP! Lv {level} (Next EXP: {expToNext})");

        UILevelUpMono.Instance?.Open();
    }

    public int GetLevel() => level;
    public int GetExp() => exp;
    public int GetExpToNext() => expToNext;
}