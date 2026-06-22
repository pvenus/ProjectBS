using UnityEngine;

[CreateAssetMenu(fileName = "VFX_RangedHit", menuName = "BS/VFX/Ranged Hit")]
public class VFX_RangedHit : ScriptableObject
{
    [Header("Prefab")]
    public GameObject prefab;

    [Header("Transform")]
    public Vector3 localOffset = Vector3.zero;
    public Vector3 scale = Vector3.one;

    [Header("Behavior")]
    public float lifetime = 0.18f;
    public bool randomZRotation = true;
    public bool attachToTarget = false;

    /// <summary>
    /// Spawn hit VFX at the target position.
    /// </summary>
    public void Play(Transform target)
    {
        if (prefab == null || target == null)
            return;

        Vector3 pos = target.position + localOffset;
        Quaternion rot = Quaternion.identity;

        if (randomZRotation)
        {
            float angle = Random.Range(0f, 360f);
            rot = Quaternion.Euler(0f, 0f, angle);
        }

        GameObject go = Instantiate(prefab, pos, rot);
        go.transform.localScale = scale;

        if (attachToTarget)
            go.transform.SetParent(target, true);

        if (lifetime > 0f)
            Destroy(go, lifetime);
    }

    /// <summary>
    /// Spawn hit VFX at an arbitrary world position.
    /// Useful when the impact point is not exactly the target pivot.
    /// </summary>
    public void PlayAt(Vector3 worldPos)
    {
        if (prefab == null)
            return;

        Quaternion rot = Quaternion.identity;
        if (randomZRotation)
        {
            float angle = Random.Range(0f, 360f);
            rot = Quaternion.Euler(0f, 0f, angle);
        }

        GameObject go = Instantiate(prefab, worldPos + localOffset, rot);
        go.transform.localScale = scale;

        if (lifetime > 0f)
            Destroy(go, lifetime);
    }
}
