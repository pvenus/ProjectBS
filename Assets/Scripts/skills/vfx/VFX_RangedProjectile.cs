using UnityEngine;

[CreateAssetMenu(fileName = "VFX_RangedProjectile", menuName = "BS/VFX/Ranged Projectile")]
public class VFX_RangedProjectile : ScriptableObject
{
    [Header("Prefab")]
    public GameObject prefab;

    [Header("Movement")]
    public float speed = 10f;
    public bool rotateToDirection = true;

    [Header("Transform")]
    public Vector3 localOffset = Vector3.zero;
    public Vector3 scale = Vector3.one;

    [Header("Lifetime")]
    public float maxLifetime = 2f;

    /// <summary>
    /// Spawn projectile VFX moving from caster toward target.
    /// </summary>
    public void Play(Transform caster, Transform target)
    {
        if (prefab == null || caster == null || target == null)
            return;

        Vector3 start = caster.position + localOffset;
        Vector3 dir = (target.position - start).normalized;

        GameObject go = Instantiate(prefab, start, Quaternion.identity);
        go.transform.localScale = scale;

        if (rotateToDirection && dir.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        var runner = go.AddComponent<RangedProjectileVfxRunner>();
        runner.Init(dir, speed, maxLifetime);
    }

    private class RangedProjectileVfxRunner : MonoBehaviour
    {
        private Vector3 _dir;
        private float _speed;
        private float _life;

        public void Init(Vector3 dir, float speed, float lifetime)
        {
            _dir = dir;
            _speed = speed;
            _life = lifetime;
        }

        private void Update()
        {
            transform.position += _dir * _speed * Time.deltaTime;

            _life -= Time.deltaTime;
            if (_life <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
