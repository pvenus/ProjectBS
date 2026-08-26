using System.Collections.Generic;
using UnityEngine;

namespace Battle.Presentation.Ambient
{
    public sealed class BattleAmbientController : MonoBehaviour
    {
        public enum AmbientKind
        {
            BirdFlock,
            WindGust,
            DryLeaves,
            GrassTuft
        }

        [System.Serializable]
        public sealed class AmbientEntry
        {
            public AmbientKind kind;
            public Sprite[] frames;
            [Min(1f)] public float framesPerSecond = 8f;
            [Min(0.01f)] public float scale = 1f;
            [Min(0.1f)] public float minLifetime = 4f;
            [Min(0.1f)] public float maxLifetime = 8f;
            [Min(0f)] public float weight = 1f;
        }

        [SerializeField] private List<AmbientEntry> entries = new();
        [SerializeField, Min(0f)] private float initialDelay = 1.5f;
        [SerializeField, Min(0.1f)] private float minSpawnInterval = 2.5f;
        [SerializeField, Min(0.1f)] private float maxSpawnInterval = 5.5f;
        [SerializeField, Min(1)] private int maxActiveActors = 24;
        [SerializeField, Min(1)] private int maxPersistentGrass = 8;
        [SerializeField, Min(0f)] private float birdGroupWeight = 0.3f;
        [SerializeField, Min(0f)] private float windLeafGroupWeight = 1f;
        [SerializeField, Min(0f)] private float grassGroupWeight = 0.55f;
        [Header("Wind + Leaf Group")]
        [SerializeField, Min(0.1f)] private float minWindLeafGroupLifetime = 6f;
        [SerializeField, Min(0.1f)] private float maxWindLeafGroupLifetime = 10f;
        [SerializeField] private Vector2 windLifetimeRatio = new(0.55f, 0.9f);
        [SerializeField] private Vector2 leafLifetimeRatio = new(0.4f, 0.85f);
        [SerializeField, Range(0f, 1f)] private float windSpeedVariance = 0.15f;
        [SerializeField, Range(0f, 1f)] private float leafSpeedVariance = 0.35f;
        [SerializeField] private Vector2 viewportPadding = new(0.08f, 0.08f);
        [SerializeField] private int backgroundSortingOrder = -800;
        [SerializeField] private int foregroundSortingOrder = 800;

        private readonly List<BattleAmbientActor> activeActors = new();
        private Camera targetCamera;
        private float nextSpawnTime;

        private void OnEnable()
        {
            targetCamera = Camera.main;
            nextSpawnTime = Time.time + initialDelay;
        }

        private void Update()
        {
            activeActors.RemoveAll(actor => actor == null);

            if (Time.time < nextSpawnTime || activeActors.Count >= maxActiveActors)
            {
                return;
            }

            targetCamera ??= Camera.main;
            if (targetCamera != null)
            {
                SpawnRandomGroup();
            }

            nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
        }

        public void Configure(
            Sprite[] birdFlock,
            Sprite[] windGust,
            Sprite[] dryLeaves,
            Sprite[] grassTuft)
        {
            entries = new List<AmbientEntry>
            {
                CreateEntry(AmbientKind.BirdFlock, birdFlock, 7f, 0.22f, 7f, 11f, 0.65f),
                CreateEntry(AmbientKind.WindGust, windGust, 10f, 0.34f, 3f, 5f, 1.2f),
                CreateEntry(AmbientKind.DryLeaves, dryLeaves, 7f, 0.18f, 4f, 7f, 1f),
                CreateEntry(AmbientKind.GrassTuft, grassTuft, 6f, 0.24f, 8f, 14f, 1.4f)
            };
        }

        private static AmbientEntry CreateEntry(
            AmbientKind kind,
            Sprite[] frames,
            float framesPerSecond,
            float scale,
            float minLifetime,
            float maxLifetime,
            float weight)
        {
            return new AmbientEntry
            {
                kind = kind,
                frames = frames,
                framesPerSecond = framesPerSecond,
                scale = scale,
                minLifetime = minLifetime,
                maxLifetime = maxLifetime,
                weight = weight
            };
        }

        private void SpawnRandomGroup()
        {
            float totalWeight = birdGroupWeight
                + windLeafGroupWeight
                + grassGroupWeight;
            if (totalWeight <= 0f)
            {
                return;
            }

            float selection = Random.value * totalWeight;
            if (selection < birdGroupWeight)
            {
                SpawnBirdGroup();
                return;
            }

            selection -= birdGroupWeight;
            if (selection < windLeafGroupWeight)
            {
                SpawnWindLeafGroup();
                return;
            }

            SpawnGrassGroup();
        }

        private static bool HasFrames(AmbientEntry entry)
        {
            return entry?.frames != null
                && entry.frames.Length > 0
                && entry.frames[0] != null;
        }

        private AmbientEntry FindEntry(AmbientKind kind)
        {
            return entries.Find(entry => entry != null
                && entry.kind == kind
                && HasFrames(entry));
        }

        private void SpawnBirdGroup()
        {
            AmbientEntry birds = FindEntry(AmbientKind.BirdFlock);
            if (!HasFrames(birds))
            {
                return;
            }

            float direction = Random.value < 0.5f ? -1f : 1f;
            float groupSpeed = Random.Range(1.4f, 2.8f);
            int count = Mathf.Min(Random.Range(1, 4), RemainingCapacity());
            float baseY = Random.Range(0.8f, 0.95f);
            for (int index = 0; index < count; index++)
            {
                Vector2 viewport = new(
                    direction > 0f
                        ? -viewportPadding.x - index * 0.055f
                        : 1f + viewportPadding.x + index * 0.055f,
                    Mathf.Clamp(baseY + Random.Range(-0.035f, 0.035f), 0.8f, 0.97f));
                SpawnActor(
                    birds,
                    viewport,
                    direction,
                    groupSpeed * Random.Range(0.88f, 1.12f),
                    false,
                    backgroundSortingOrder + index);
            }
        }

        private void SpawnWindLeafGroup()
        {
            AmbientEntry wind = FindEntry(AmbientKind.WindGust);
            AmbientEntry leaves = FindEntry(AmbientKind.DryLeaves);
            if (!HasFrames(wind) || !HasFrames(leaves))
            {
                return;
            }

            float direction = Random.value < 0.5f ? -1f : 1f;
            float groupSpeed = Random.Range(0.9f, 1.8f);
            float groupLifetime = Random.Range(
                minWindLeafGroupLifetime,
                Mathf.Max(minWindLeafGroupLifetime, maxWindLeafGroupLifetime));
            float baseY = Random.Range(0.12f, 0.63f);
            GameObject groupObject = new("Ambient_WindLeafGroup");
            groupObject.transform.SetParent(transform, false);
            BattleAmbientGroup group = groupObject.AddComponent<BattleAmbientGroup>();
            group.Initialize(groupLifetime);

            int windCount = Mathf.Min(Random.Range(1, 3), RemainingCapacity());
            for (int index = 0; index < windCount; index++)
            {
                SpawnActor(
                    wind,
                    GetEdgeViewport(direction, baseY + Random.Range(-0.045f, 0.045f), index),
                    direction,
                    RandomAround(groupSpeed, windSpeedVariance),
                    false,
                    backgroundSortingOrder + 20 + index,
                    groupObject.transform,
                    RandomLifetime(groupLifetime, windLifetimeRatio));
            }

            int leafCount = Mathf.Min(Random.Range(2, 6), RemainingCapacity());
            for (int index = 0; index < leafCount; index++)
            {
                SpawnActor(
                    leaves,
                    GetEdgeViewport(direction, baseY + Random.Range(-0.1f, 0.1f), index),
                    direction,
                    RandomAround(groupSpeed, leafSpeedVariance),
                    false,
                    foregroundSortingOrder + index,
                    groupObject.transform,
                    RandomLifetime(groupLifetime, leafLifetimeRatio));
            }
        }

        private void SpawnGrassGroup()
        {
            AmbientEntry grass = FindEntry(AmbientKind.GrassTuft);
            int grassCount = activeActors.FindAll(
                actor => actor != null && actor.Kind == AmbientKind.GrassTuft).Count;
            if (!HasFrames(grass) || grassCount >= maxPersistentGrass)
            {
                return;
            }

            int count = Mathf.Min(
                Random.Range(2, 5),
                Mathf.Min(maxPersistentGrass - grassCount, RemainingCapacity()));
            for (int index = 0; index < count; index++)
            {
                Vector2 viewport = new(
                    Random.Range(viewportPadding.x, 1f - viewportPadding.x),
                    Random.Range(0.05f, 0.66f));
                SpawnActor(
                    grass,
                    viewport,
                    0f,
                    0f,
                    true,
                    backgroundSortingOrder + 100 + Mathf.RoundToInt(viewport.y * 100f));
            }
        }

        private void SpawnActor(
            AmbientEntry entry,
            Vector2 viewportPosition,
            float direction,
            float movementSpeed,
            bool infiniteLifetime,
            int sortingOrder,
            Transform parent = null,
            float lifetimeOverride = -1f)
        {
            if (!HasFrames(entry) || RemainingCapacity() <= 0)
            {
                return;
            }

            Vector3 position = targetCamera.ViewportToWorldPoint(
                new Vector3(
                    viewportPosition.x,
                    Mathf.Clamp(viewportPosition.y, 0.02f, 0.98f),
                    -targetCamera.transform.position.z));
            position.z = 0f;
            GameObject actorObject = new($"Ambient_{entry.kind}");
            actorObject.transform.SetParent(parent != null ? parent : transform, false);
            actorObject.transform.position = position;

            SpriteRenderer renderer = actorObject.AddComponent<SpriteRenderer>();
            renderer.sprite = entry.frames[0];
            renderer.sortingOrder = sortingOrder;
            renderer.flipX = direction < 0f;

            BattleAmbientActor actor = actorObject.AddComponent<BattleAmbientActor>();
            actor.Initialize(
                entry.kind,
                entry.frames,
                entry.framesPerSecond,
                targetCamera,
                lifetimeOverride > 0f
                    ? lifetimeOverride
                    : Random.Range(entry.minLifetime, entry.maxLifetime),
                entry.scale,
                viewportPadding,
                direction,
                movementSpeed,
                infiniteLifetime);
            activeActors.Add(actor);
        }

        private Vector2 GetEdgeViewport(float direction, float y, int groupIndex)
        {
            float offset = groupIndex * 0.035f;
            float x = direction > 0f
                ? -viewportPadding.x - offset
                : 1f + viewportPadding.x + offset;
            return new Vector2(x, Mathf.Clamp(y, 0.05f, 0.66f));
        }

        private int RemainingCapacity()
        {
            return Mathf.Max(0, maxActiveActors - activeActors.Count);
        }

        private static float RandomAround(float baseline, float variance)
        {
            float safeVariance = Mathf.Clamp01(variance);
            return baseline * Random.Range(1f - safeVariance, 1f + safeVariance);
        }

        private static float RandomLifetime(float groupLifetime, Vector2 ratioRange)
        {
            float minimum = Mathf.Clamp01(Mathf.Min(ratioRange.x, ratioRange.y));
            float maximum = Mathf.Clamp(
                Mathf.Max(ratioRange.x, ratioRange.y),
                minimum,
                1f);
            return groupLifetime * Random.Range(minimum, maximum);
        }
    }
}
