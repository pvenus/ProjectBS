using System.Collections.Generic;
using Skill;
using UnityEngine;

namespace Character
{
    [DisallowMultipleComponent]
    public sealed class NpcCastGroundConeTelegraphMono : MonoBehaviour
    {
        private const string BlackKnifeCut =
            "skill.character.black_cloth_raider.1.basic_attack.black_knife_cut";
        private const string ChainSwing =
            "skill.character.chain_dragger_raider.1.basic_attack.chain_swing";
        private const string RootName = "__NpcCastGroundConeTelegraph";
        private const int SectorSegments = 20;
        private const float BoundaryThickness = .018f;
        private static readonly Color FillStartColor = new(.42f, .035f, .025f, .07f);
        private static readonly Color FillFullColor = new(.72f, .06f, .035f, .22f);
        private static readonly Color BoundaryStartColor = new(.82f, .16f, .09f, .34f);
        private static readonly Color BoundaryFullColor = new(.96f, .25f, .12f, .62f);

        private GameObject presentationRoot;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh mesh;
        private Material material;
        private Vector2 origin;
        private Vector2 direction;
        private Vector2 snapshotAttackPoint;
        private float length;
        private float halfAngleDegrees;
        private bool showing;
        private bool warningLogged;

        public bool IsShowing => showing;

        public static bool Supports(EquipmentSkillRuntimeData runtime)
        {
            string id = runtime?.sourceEquipment?.EquipmentId;
            return string.Equals(id, BlackKnifeCut, System.StringComparison.Ordinal) ||
                string.Equals(id, ChainSwing, System.StringComparison.Ordinal);
        }

        public static float ResolveFootprintLength(
            float spawnOffset,
            float range,
            float worldColliderRadius)
        {
            return Mathf.Max(.01f,
                Mathf.Max(0f, spawnOffset) +
                Mathf.Max(0f, range) +
                Mathf.Max(.01f, worldColliderRadius));
        }

        public static float ResolveHalfAngleDegrees(
            float footprintLength,
            float worldColliderRadius)
        {
            float safeLength = Mathf.Max(.01f, footprintLength);
            float safeRadius = Mathf.Max(.01f, worldColliderRadius);
            return Mathf.Clamp(
                Mathf.Asin(Mathf.Clamp01(safeRadius / safeLength)) * Mathf.Rad2Deg,
                5f,
                65f);
        }

        public bool Begin(
            Vector2 worldOrigin,
            Vector2 snapshotDirection,
            float skillRange,
            float projectileSpawnOffset,
            float projectileScale,
            float colliderRadius)
        {
            HideAndReset();
            if (skillRange <= 0f || !isActiveAndEnabled || !EnsurePresentationObjects())
            {
                WarnOnce("missing or inactive presentation capability; gameplay continues");
                return false;
            }

            try
            {
                origin = worldOrigin;
                snapshotAttackPoint = worldOrigin + snapshotDirection;
                direction = snapshotDirection.sqrMagnitude > .0001f
                    ? snapshotDirection.normalized
                    : Vector2.right;
                float worldColliderRadius = Mathf.Max(.01f, colliderRadius) *
                    Mathf.Max(.01f, projectileScale);
                length = ResolveFootprintLength(
                    projectileSpawnOffset,
                    skillRange,
                    worldColliderRadius);
                halfAngleDegrees = ResolveHalfAngleDegrees(
                    length,
                    worldColliderRadius);
                showing = true;
                presentationRoot.SetActive(true);
                SetProgress(0f);
                return true;
            }
            catch (System.Exception exception)
            {
                HideAndReset();
                WarnOnce($"presentation begin failed ({exception.GetType().Name}); gameplay continues");
                return false;
            }
        }

        public void SetProgress(float progress)
        {
            if (!showing || mesh == null || meshRenderer == null || material == null)
                return;

            float value = Mathf.Clamp01(progress);
            RefreshAimFromCurrentCaster();
            RebuildMesh(value);
            material.color = Color.white;
        }

        public void CompleteAndHide() => HideAndReset();
        public void CancelAndHide() => HideAndReset();

        public void HideAndReset()
        {
            showing = false;
            if (mesh != null)
                mesh.Clear();
            if (presentationRoot != null)
                presentationRoot.SetActive(false);
            origin = Vector2.zero;
            direction = Vector2.right;
            snapshotAttackPoint = Vector2.zero;
            length = 0f;
            halfAngleDegrees = 0f;
        }

        private bool EnsurePresentationObjects()
        {
            try
            {
                if (presentationRoot == null)
                {
                    Transform existing = transform.Find(RootName);
                    presentationRoot = existing != null
                        ? existing.gameObject
                        : new GameObject(RootName);
                    presentationRoot.transform.SetParent(transform, false);
                }

                // The mesh vertices are converted from world space through the
                // actor transform. Keep the owned child identity so pooled or
                // domain-reloaded objects cannot apply that transform twice.
                presentationRoot.transform.localPosition = Vector3.zero;
                presentationRoot.transform.localRotation = Quaternion.identity;
                presentationRoot.transform.localScale = Vector3.one;

                // Use Unity's overloaded null check instead of ?? so a destroyed
                // component reference from domain reload/pool reuse self-heals.
                meshFilter = presentationRoot.GetComponent<MeshFilter>();
                if (meshFilter == null)
                    meshFilter = presentationRoot.AddComponent<MeshFilter>();
                meshRenderer = presentationRoot.GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                    meshRenderer = presentationRoot.AddComponent<MeshRenderer>();
                if (meshFilter == null || meshRenderer == null)
                    return false;

                if (mesh == null)
                {
                    mesh = new Mesh
                    {
                        name = "NpcCastGroundConeRuntimeMesh",
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    mesh.MarkDynamic();
                }
                if (mesh == null)
                    return false;
                meshFilter.sharedMesh = mesh;

                if (material == null)
                {
                    Shader shader = Shader.Find("Sprites/Default");
                    if (shader == null)
                        return false;
                    material = new Material(shader)
                    {
                        name = "NpcCastGroundConeRuntimeMaterial",
                        hideFlags = HideFlags.HideAndDontSave
                    };
                }
                if (material == null)
                    return false;
                meshRenderer.sharedMaterial = material;
                // Reapply on every acquire so a pooled/domain-reloaded renderer
                // cannot retain an unrelated presentation order.
                meshRenderer.sortingLayerID = 0;
                meshRenderer.sortingOrder = Battle.BattlePresentationSortingPolicy.GroundTelegraph;
                return true;
            }
            catch (System.Exception exception)
            {
                meshFilter = null;
                meshRenderer = null;
                WarnOnce($"presentation repair failed ({exception.GetType().Name}); gameplay continues");
                return false;
            }
        }

        private void WarnOnce(string message)
        {
            if (warningLogged)
                return;
            warningLogged = true;
            Debug.LogWarning($"[NpcCastGroundConeTelegraph] {name}: {message}.", this);
        }

        private void RebuildMesh(float progress)
        {
            List<Vector3> vertices = new(160);
            List<Color> colors = new(160);
            List<int> triangles = new(240);
            Color fillColor = Color.Lerp(FillStartColor, FillFullColor, progress);
            Color boundaryColor = Color.Lerp(BoundaryStartColor, BoundaryFullColor, progress);
            float fillRadius = length * Mathf.Lerp(.12f, 1f, progress);

            // True sector: every fill triangle shares the exact caster apex.
            for (int index = 0; index < SectorSegments; index++)
            {
                float t0 = index / (float)SectorSegments;
                float t1 = (index + 1) / (float)SectorSegments;
                Vector2 ray0 = Rotate(direction, Mathf.Lerp(-halfAngleDegrees, halfAngleDegrees, t0));
                Vector2 ray1 = Rotate(direction, Mathf.Lerp(-halfAngleDegrees, halfAngleDegrees, t1));
                AddTriangle(vertices, colors, triangles,
                    origin, origin + ray0 * fillRadius, origin + ray1 * fillRadius, fillColor);
            }

            Vector2 leftRay = Rotate(direction, halfAngleDegrees);
            Vector2 rightRay = Rotate(direction, -halfAngleDegrees);
            AddLine(vertices, colors, triangles, origin, origin + leftRay * length,
                BoundaryThickness, boundaryColor);
            AddLine(vertices, colors, triangles, origin, origin + rightRay * length,
                BoundaryThickness, boundaryColor);

            float innerRadius = Mathf.Max(0f, length - BoundaryThickness);
            for (int index = 0; index < SectorSegments; index++)
            {
                float t0 = index / (float)SectorSegments;
                float t1 = (index + 1) / (float)SectorSegments;
                Vector2 ray0 = Rotate(direction, Mathf.Lerp(-halfAngleDegrees, halfAngleDegrees, t0));
                Vector2 ray1 = Rotate(direction, Mathf.Lerp(-halfAngleDegrees, halfAngleDegrees, t1));
                AddQuad(vertices, colors, triangles,
                    origin + ray0 * innerRadius,
                    origin + ray0 * length,
                    origin + ray1 * length,
                    origin + ray1 * innerRadius,
                    boundaryColor);
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }

        private void AddTriangle(
            List<Vector3> vertices,
            List<Color> colors,
            List<int> triangles,
            Vector2 a,
            Vector2 b,
            Vector2 c,
            Color color)
        {
            int start = vertices.Count;
            vertices.Add(ToLocal(a));
            vertices.Add(ToLocal(b));
            vertices.Add(ToLocal(c));
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            triangles.Add(start);
            triangles.Add(start + 1);
            triangles.Add(start + 2);
        }

        private void AddLine(
            List<Vector3> vertices,
            List<Color> colors,
            List<int> triangles,
            Vector2 start,
            Vector2 end,
            float width,
            Color color)
        {
            Vector2 delta = end - start;
            if (delta.sqrMagnitude <= .000001f) return;
            Vector2 normal = new(-delta.y, delta.x);
            normal = normal.normalized * (width * .5f);
            AddQuad(vertices, colors, triangles,
                start - normal, start + normal, end + normal, end - normal, color);
        }

        private void AddQuad(
            List<Vector3> vertices,
            List<Color> colors,
            List<int> triangles,
            Vector2 a,
            Vector2 b,
            Vector2 c,
            Vector2 d,
            Color color)
        {
            int start = vertices.Count;
            vertices.Add(ToLocal(a));
            vertices.Add(ToLocal(b));
            vertices.Add(ToLocal(c));
            vertices.Add(ToLocal(d));
            for (int index = 0; index < 4; index++) colors.Add(color);
            triangles.Add(start);
            triangles.Add(start + 1);
            triangles.Add(start + 2);
            triangles.Add(start);
            triangles.Add(start + 2);
            triangles.Add(start + 3);
        }

        private static Vector2 Rotate(Vector2 value, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2(
                value.x * cos - value.y * sin,
                value.x * sin + value.y * cos);
        }

        private void RefreshAimFromCurrentCaster()
        {
            if (!showing) return;

            origin = transform.position;
            Vector2 toSnapshot = snapshotAttackPoint - origin;
            float distance = toSnapshot.magnitude;
            if (distance > .0001f)
            {
                direction = toSnapshot / distance;
            }
            // Radius and sector shape are cast-start invariants. Knockback only
            // translates the apex and rotates the axis; it never stretches mesh.
            // A zero-length aim retains the last valid direction.
        }

        private Vector3 ToLocal(Vector2 worldPoint) =>
            transform.InverseTransformPoint(new Vector3(worldPoint.x, worldPoint.y, 0f));

        private void OnDisable() => HideAndReset();

        private void OnDestroy()
        {
            HideAndReset();
            if (mesh != null)
                Destroy(mesh);
            if (material != null)
                Destroy(material);
        }
    }

    [DisallowMultipleComponent]
    public sealed class NpcVisualScalePresentationMono : MonoBehaviour
    {
        private const string ProxyName = "__NpcVisualScaleProxy";
        private const float BlackClothMultiplier = 1.3982143f;
        private const float ChainDraggerMultiplier = 1.35f;
        private SpriteRenderer sourceRenderer;
        private SpriteRenderer proxyRenderer;
        private bool sourceEnabledBeforeProxy;
        private float multiplier = 1f;
        private MaterialPropertyBlock propertyBlock;

        public static float ResolveMultiplier(string characterId) => characterId switch
        {
            "character.black_cloth_raider.1" => BlackClothMultiplier,
            "character.chain_dragger_raider.1" => ChainDraggerMultiplier,
            _ => 1f
        };

        public void Configure(string characterId)
        {
            multiplier = ResolveMultiplier(characterId);
            if (Mathf.Approximately(multiplier, 1f))
            {
                RestoreAndHide();
                enabled = false;
                return;
            }

            enabled = true;
            AcquireRenderers();
            ApplyPresentation();
        }

        private void LateUpdate()
        {
            AcquireRenderers();
            ApplyPresentation();
        }

        private void AcquireRenderers()
        {
            if (sourceRenderer == null)
            {
                sourceRenderer = GetComponent<SpriteRenderer>();
                if (sourceRenderer == null)
                {
                    SpriteRenderer[] candidates = GetComponentsInChildren<SpriteRenderer>(true);
                    for (int i = 0; i < candidates.Length; i++)
                    {
                        if (candidates[i] != null &&
                            candidates[i].GetComponent<NpcVisualScaleProxyMarkerMono>() == null)
                        {
                            sourceRenderer = candidates[i];
                            break;
                        }
                    }
                }
            }

            Transform proxy = transform.Find(ProxyName);
            if (proxy == null)
            {
                proxy = new GameObject(ProxyName).transform;
                proxy.SetParent(transform, false);
                proxy.gameObject.AddComponent<NpcVisualScaleProxyMarkerMono>();
            }

            proxyRenderer = proxy.GetComponent<SpriteRenderer>();
            if (proxyRenderer == null)
            {
                proxyRenderer = proxy.gameObject.AddComponent<SpriteRenderer>();
                if (proxyRenderer != null)
                    proxyRenderer.enabled = false;
            }
        }

        private void ApplyPresentation()
        {
            if (sourceRenderer == null || proxyRenderer == null)
            {
                RestoreAndHide();
                return;
            }

            if (!proxyRenderer.enabled)
                sourceEnabledBeforeProxy = sourceRenderer.enabled;
            proxyRenderer.sprite = sourceRenderer.sprite;
            proxyRenderer.sharedMaterial = sourceRenderer.sharedMaterial;
            proxyRenderer.color = sourceRenderer.color;
            proxyRenderer.flipX = sourceRenderer.flipX;
            proxyRenderer.flipY = sourceRenderer.flipY;
            proxyRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
            proxyRenderer.sortingOrder = sourceRenderer.sortingOrder;
            proxyRenderer.maskInteraction = sourceRenderer.maskInteraction;
            propertyBlock ??= new MaterialPropertyBlock();
            sourceRenderer.GetPropertyBlock(propertyBlock);
            proxyRenderer.SetPropertyBlock(propertyBlock);
            proxyRenderer.transform.localPosition = Vector3.zero;
            proxyRenderer.transform.localRotation = Quaternion.identity;
            proxyRenderer.transform.localScale = new Vector3(multiplier, multiplier, 1f);
            proxyRenderer.enabled = sourceEnabledBeforeProxy;
            sourceRenderer.enabled = false;
        }

        private void RestoreAndHide()
        {
            if (sourceRenderer != null)
                sourceRenderer.enabled = sourceEnabledBeforeProxy;
            if (proxyRenderer != null)
            {
                proxyRenderer.enabled = false;
                proxyRenderer.sprite = null;
                proxyRenderer.transform.localScale = Vector3.one;
            }
        }

        private void OnDisable() => RestoreAndHide();
        private void OnDestroy() => RestoreAndHide();
    }

    [DisallowMultipleComponent]
    public sealed class NpcVisualScaleProxyMarkerMono : MonoBehaviour
    {
    }
}
