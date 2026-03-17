using UnityEngine;

[CreateAssetMenu(fileName = "VFX_RangedCast", menuName = "BS/VFX/Ranged Cast")]
public class VFX_RangedCast : ScriptableObject
{
    [Header("Prefab")]
    public GameObject prefab;

    [Header("Transform")]
    public Vector3 localOffset = Vector3.zero;
    public Vector3 scale = Vector3.one;

    [Header("Behavior")]
    public float lifetime = 0.15f;
    public bool rotateToDirection = true;

    /// <summary>
    /// Spawn the cast VFX at the caster position.
    /// </summary>
    public void Play(Transform caster, Vector3 direction)
    {
        if (prefab == null || caster == null)
            return;

        Vector3 pos = caster.position + localOffset;

        GameObject go = Instantiate(prefab, pos, Quaternion.identity);

        if (rotateToDirection && direction.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        go.transform.localScale = scale;

        if (lifetime > 0f)
            Destroy(go, lifetime);
    }
}
