using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple projectile behaviour.
/// </summary>
public class ProjectileMono : MonoBehaviour
{
    private Vector2 _direction;
    private float _speed;
    private float _maxDistance;
    private float _lifetime;
    private int _damage;

    private Vector2 _startPos;
    private float _aliveTime;

    private Rigidbody2D _rb;

    // Tracks which NPCs have already been damaged by THIS projectile.
    private readonly HashSet<int> _damagedNpcIds = new HashSet<int>();

    public void Initialize(Vector2 dir, float speed, float maxDist, float lifetime, int damage)
    {
        _direction = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.up;
        _speed = speed;
        _maxDistance = maxDist;
        _lifetime = lifetime;
        _damage = damage;

        _startPos = transform.position;
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // Move using physics step for more reliable trigger detection.
        float delta = _speed * Time.fixedDeltaTime;
        Vector2 nextPos = (Vector2)transform.position + (_direction * delta);

        if (_rb != null)
            _rb.MovePosition(nextPos);
        else
            transform.position = nextPos;

        _aliveTime += Time.fixedDeltaTime;

        float traveled = Vector2.Distance(_startPos, (Vector2)transform.position);
        if (_aliveTime >= _lifetime || traveled >= _maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Find an NPC on the other object or its parents.
        var npc = other.GetComponentInParent<NpcMono>();
        if (npc == null) return;

        int id = npc.GetInstanceID();
        if (_damagedNpcIds.Contains(id)) return; // already damaged this NPC

        _damagedNpcIds.Add(id);
        npc.TakeDamage(_damage);
    }
}
